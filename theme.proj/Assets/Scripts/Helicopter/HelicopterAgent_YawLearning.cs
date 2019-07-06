using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.UI;

public class HelicopterAgent_YawLearning : Agent
{

    private Sensor[] Sensors { get; set; }

    private WaypointSensor PositionSensor { get; set; }

    private SimpleHelicopterController Controller { get; set; }

    private Vector3 StartPosition { get; set; }
    private Quaternion StartRotation { get; set; }

    private Vector3 LastPosition { get; set; } 
    private Vector3 StartWayPointPosition { get; set; }

    public float DriveTime { get; set; }
    private float PrevReward { get; set; }
    private float KillTime { get; set; }
    private float StopTime { get; set; }
    private float RotatingTime { get; set; }
    private float NegativeRotatingTime { get; set; }




    private Rigidbody HeliRb { get; set; }
    private RayPerception RayPer { get; set; }

    private Vector3 CurrentGoal { get; set; }
    private Vector3 ExGoal { get; set; }
    private int GoalCounter { get; set; }
    private int[] Points;
    private int[] SelectedPoints;
    private int PointNumber { get; set; }
    private float Distance { get; set; }
    private float MaxDistance { get; set; }

    private int CollisionCount { get; set; }
    private int Seed = 1;


    [SerializeField] private Text statusText = null;
    private Text StatusText { get { return statusText; } }



    GameObject sphere;


    /// <summary>
    /// 開始時に呼び出される初期化処理
    /// </summary>
    private void Start() {
        UnityEngine.Random.InitState(Seed);
        // 車の制御コントローラーを取得
        Controller = GetComponent<SimpleHelicopterController>();

        // 子に追加されているセンサーを全て取得
        Sensors = GetComponentsInChildren<Sensor>();

        // ウェイポイントセンサー取得
        PositionSensor = GetComponent<WaypointSensor>();

        HeliRb = this.transform.GetComponent<Rigidbody>();


        StartPosition = transform.position;
        StartRotation = transform.rotation;
        LastPosition = StartPosition;
        StartWayPointPosition = PositionSensor.points[0].transform.position;
        CurrentGoal = StartWayPointPosition;
        Distance = Vector3.Distance(StartPosition, CurrentGoal);

        GoalCounter = 0;
        CollisionCount = 0;
        HeliRb.isKinematic = false;


    }

    /// <summary>
    /// 毎フレーム呼び出される処理
    /// Updateではなくこちらを使用する
    /// </summary>
  /*  public void FixedUpdate() {
        // ヘリコプターコントロール
        Controller.Move(0, 1.0f, 1.0f, 100.0f);

        // ウェイポイントまでの障害物数を取得
        var hits = PositionSensor.GetObjects();

        // ウェイポイントまでの直線距離を取得
        var distance = PositionSensor.GetDistance();

        // センサーの距離をリストに追加する
        var results = new List<float>();
        Array.ForEach(Sensors, sensor => {
            results.AddRange(sensor.Hits());
        });
    }
    */
    public override void AgentReset() {
        Seed = Seed + 1;
        UnityEngine.Random.InitState(Seed);

        Controller.Stop();
        HeliRb.ResetInertiaTensor();
        Controller.EngineForce = Controller.InitEngineForce;
        PositionSensor.WaypointReset();
        transform.position = StartPosition;
        transform.rotation = StartRotation;
        LastPosition = StartPosition;
        DriveTime = 0;
        KillTime = 0;
        //StopTime = 0;
        //RotatingTime = 0;
        //NegativeRotatingTime = 0;
        SetReward(0);
        PositionSensor.points[0].transform.position = StartPosition + UnityEngine.Random.onUnitSphere * Distance;

        CollisionCount = 0;
        HeliRb.isKinematic = false;
        GoalCounter = 0;

    }

    public override List<float> CollectYawObservations()//UnityのY軸回りに関する情報を取得
    {
        var yawobservations = new List<float>();

        //Y軸周りの角速度
        yawobservations.Add(HeliRb.angularVelocity.y);

        // ウェイポイントまでの水平角度を取得
        yawobservations.Add(PositionSensor.GetHorizontalAngles()[0]);



        return yawobservations;
    }

    public override void AgentAction(float[] outputs)//動かす
    {
        float horizontalangle = PositionSensor.GetHorizontalAngles()[0];
        AddReward((1.0f - Mathf.Abs(PositionSensor.GetHorizontalAngles()[0])) * 3);


        if (Math.Abs(horizontalangle)< 0.02)
        {
            AddReward(0.5f);
        }

        PositionSensor.WaypointReset();
        Controller.Move(0, 0, 0, Mathf.Clamp(outputs[0],-1,1));

        Vector3 CurrentPosition = HeliRb.transform.position;
        float Velocity = HeliRb.velocity.magnitude;


       
        DriveTime += Time.fixedDeltaTime;

        float velocityangle = PositionSensor.GetVelocityAngles()[0];
        float perpendiculardistance = GetPerpendicularDistance();
        var w = 1-Mathf.Pow(Mathf.Min(perpendiculardistance/100 ,1),3);
        var deltay = 1-Mathf.Pow(Mathf.Min((Math.Abs(PositionSensor.GetVector()[0].y)/100),1),3);
        var d = 5*(1-Mathf.Pow(Mathf.Min(PositionSensor.GetDistance()[0]/Distance,1), 3));
        
        var angle =1- (Math.Abs(velocityangle));

        if (StatusText != null)
        {
            StatusText.text = "EngineForce : " + Controller.EngineForce + "\nTail : " + Controller.Torque + "\nReward : " + Reward + "\nTime : " + DriveTime + "\nGoalCounter : " + GoalCounter + "\nDistrance : " + PositionSensor.GetDistance()[0];
        }


        if (DriveTime > 10)//段階的に時間を延ばす
        {
            //AddReward(Mathf.Clamp((MaxDistance - PositionSensor.GetDistance()[PointNumber - 1]), 0, MaxDistance) * 10);
           // Debug.Log("Done!");
            Controller.Stop();
            Done();
            return;

        }
       
    }

    /// <summary>
    /// 衝突時に呼び出されるコールバック
    /// </summary>
    /// <param name="collision"></param>
    public void OnCollisionEnter(Collision collision) {

        // 例えばtagがwallだったらの判定
        if (collision.gameObject.tag == "wall") {
            /* if(DriveTime < 10.0f)
             {
                 // 停止
                 Controller.Stop();
                 Done();
             }
             else
             {*/
            CollisionCount += 1;
            AddReward(-5);
            //}
        }
    }



    public void OnTriggerEnter(Collider other) {
        other.gameObject.SetActive(false);
    }

    private float Clamp(float v, float min, float max)
    {
        return min + (max - min) * (v + 1.0f) / 2.0f;
    }

    private Vector3 GetPerpendicularFootPrint(Vector3 a, Vector3 b, Vector3 p)
    {
        
        return a + Vector3.Project(p- a, b - a);
    }

    private float GetPerpendicularDistance()//最短経路に下ろした垂線の長さを取得する
    {
        Vector3 a = new Vector3(0.0f, 0.0f, 0.0f);
        if (GoalCounter == 0)
        {
            a = StartPosition;
        }
        else
        {
            a = ExGoal;
        }
        Vector3 b = CurrentGoal;
        Vector3 CurrentPosition = HeliRb.transform.position;
        var h = GetPerpendicularFootPrint(a, b, CurrentPosition);
        float distance = Vector3.Distance(CurrentPosition, h);
        return distance;
    }
}

