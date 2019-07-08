using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.UI;

public class HelicopterAgent_seperate2 : Agent
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
        //Seed = Seed + 1;
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
        PositionSensor.points[0].transform.position = StartWayPointPosition;
        CurrentGoal = StartWayPointPosition;
        GoalCounter = 0;

        CollisionCount = 0;
        HeliRb.isKinematic = false;
    }

    public bool ReachWaypoint()
    {
        var distance = PositionSensor.GetDistance();
        if (distance[0] < 18.0f)
        {
            return true;
        }
        else
            return false;
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

    public override List<float> CollectObservations()//その他に関する情報を取得
    {
        var observations = new List<float>();

        var v = HeliRb.velocity;
        float velocityangle = PositionSensor.GetVelocityAngles()[0];


        //Debug.Log("velocity" + v);

        //Debug.Log(Sensors[0]);

        // センサーの距離をリストに追加する
        Array.ForEach(Sensors, sensor => {
            observations.AddRange(sensor.Hits());
        });

        //ヘリコプターの速度ベクトルを取得
        observations.Add(v.x);

        observations.Add(v.y);
   
        observations.Add(v.z);

        // ウェイポイントまでの障害物数を取得
        //observations.Add(PositionSensor.GetObjects()[CurrentGoal]);

        // ウェイポイントまでの直線距離,ベクトルを取得
        //observations.Add(PositionSensor.GetDistance()[0]);

        //ウェイポイントへのベクトルを取得
        observations.Add(PositionSensor.GetVector()[0].x);

        observations.Add(PositionSensor.GetVector()[0].y);

        observations.Add(PositionSensor.GetVector()[0].z);

        //進行方向と次のウェイポイントとの角度を取得
        observations.Add(velocityangle);


        //各軸周りの回転角
        //observations.Add(HeliRb.transform.eulerAngles.x);

       // observations.Add(HeliRb.transform.eulerAngles.y);

        //observations.Add(HeliRb.transform.eulerAngles.z);

        //各軸周りの角速度
        //observations.Add(HeliRb.angularVelocity.x);

        //observations.Add(HeliRb.angularVelocity.y);

        //observations.Add(HeliRb.angularVelocity.z);


        // ウェイポイントまでの水平角度を取得
        //observations.Add(PositionSensor.GetHorizontalAngles()[CurrentGoal]);

        return observations;
    }

    public override void AgentAction(float[] outputs1, float[] outputs2)//動かす
    {
        PositionSensor.WaypointReset();
        Controller.Move(Mathf.Clamp(outputs1[0],-1,1), Mathf.Clamp(outputs1[1], -1,1), Mathf.Clamp(outputs1[2],-1,1), Mathf.Clamp(outputs2[0],-1,1));

        if (ReachWaypoint())//ウェイポイントに到達したときの処理
        {   
            ExGoal = CurrentGoal;
            CurrentGoal = CurrentGoal + UnityEngine.Random.onUnitSphere * Distance;
            PositionSensor.points[0].transform.position = CurrentGoal;
            //var timebonus = 5*Mathf.Pow(12 * (GoalCounter + 1) - DriveTime,2);
            //AddReward(timebonus);
            GoalCounter++;
            AddReward(1000);

        }



        Vector3 CurrentPosition = HeliRb.transform.position;
        float Velocity = HeliRb.velocity.magnitude;


        float waypointdistance = PositionSensor.GetDistance()[0];
        int waypointobject = PositionSensor.GetObjects()[0];
        float horizontalangle = PositionSensor.GetHorizontalAngles()[0];
        //float normalizeddistance = Mathf.Clamp(((Distance_to_next_waypoint - PositionSensor.GetDistance()[CurrentGoal])/ Distance_to_next_waypoint),0,1);

        //AddReward((1.0f - Mathf.Abs(PositionSensor.GetHorizontalAngles()[CurrentGoal]))*3);
        //AddReward(Mathf.Clamp(((Distance_to_next_waypoint - PositionSensor.GetDistance()[CurrentGoal])/Distance_to_next_waypoint),0,1)*2);
       
        DriveTime += Time.fixedDeltaTime;

        //Debug.Log(DriveTime);

        /*
        if (CurrentPosition.y > 160.0f)//高く上がりすぎるものにペナルティ
        {
            //Debug.Log("too heigh");
            AddReward(-100f);
            Done();
            return;
        }
        */

        /*     if (DriveTime >= 5.0f)//時間がたっても報酬が増えないものを消す
             {
                 if (Reward < 5)
                 {
                     Debug.Log("no reward");
                     Controller.Stop();
                     Done();
                     return;
                 }
             }
             */
        /*
        if (Velocity < 5.0f)//動かないものにペナルティ
         {
             StopTime += Time.fixedDeltaTime;
             if (StopTime > 4)
             {
                 //Debug.Log("Don't move");
                 AddReward(-0.5f);
             }
         }
         */
        if(PositionSensor.GetDistance()[0] > Distance*2.5)//離れすぎるもを強制終了
        {
            Controller.Stop();
            Done();
            return;
        }
        /*
        if (Controller.Torque > 0.8)
        {
            RotatingTime += Time.fixedDeltaTime;
            if (RotatingTime > 10)
            {
                AddReward(-0.5f);
            }
        }
        */
        /*
        if (Controller.Torque < -0.8)
        {
            NegativeRotatingTime += Time.fixedDeltaTime;
            if (NegativeRotatingTime > 10)
            {
                AddReward(-0.5f);
            }
        }
        */
        if (transform.position.y <= 3.0f)//地面に接触し続けるものを消す
        {
            AddReward(-1.0f);
        }
        /*
        if (Controller.IsOnGround)
        {
            AddReward(-0.02f);
        }
        */
        /*
        if (Controller.EngineForce < 0.1)
        {
            AddReward(-0.02f);
        }
        */
        float velocityangle = PositionSensor.GetVelocityAngles()[0];
        float perpendiculardistance = GetPerpendicularDistance();
        var w = 1-Mathf.Pow(Mathf.Min(perpendiculardistance/80 ,1),3);
        var deltay = 1-Mathf.Pow(Mathf.Min((Math.Abs(PositionSensor.GetVector()[0].y)/100),1),3);
        var d = 5*(1-Mathf.Pow(Mathf.Min(PositionSensor.GetDistance()[0]/Distance,1), 3));
        
        var angle =1- (Math.Abs(velocityangle));

        AddReward(d+deltay);//ウェイポイントまでの距離が近いほうが良い

        if (StatusText != null)
        {
            StatusText.text = "EngineForce : " + Controller.EngineForce + "\nTail : " + Controller.Torque + "\nReward : " + Reward + "\nTime : " + DriveTime + "\nGoalCounter : " + GoalCounter + "\nDistrance : " + PositionSensor.GetDistance()[0];
        }


        if (DriveTime > 13*(GoalCounter+1))//段階的に時間を延ばす
        {
            //AddReward(Mathf.Clamp((MaxDistance - PositionSensor.GetDistance()[PointNumber - 1]), 0, MaxDistance) * 10);
           // Debug.Log("Done!");
            Controller.Stop();
            //if(TrialCount == PointsPerTrial)
            //{
                //Reward = Reward / PointsPerTrial;//平均で考える。
            Done();
            return;
            //}
            //TrialCount++;
            //Restart();
            //return;
        }
       
        if (DriveTime > 500)
        {
            Controller.Stop();
            Done();
            return;
        }
        
        /*if (DriveTime >= 10.0f)//時間がたっても報酬が増えないものを消す
        {
            if (Reward < 10)
            {
                Controller.Stop();
                Done();
                return;
            }
        }
        */

        /*
        if ((x*x + y*y + z*z) < 1.0F)
        {
            Controller.Stop();
            Done();
            return;
        }

        if ((int)PrevReward == (int)Reward)
        {//報酬が増えないフレームが一定数以上になるものを消す
            KillTime += Time.fixedDeltaTime;
            if (KillTime >= 10.0f)
            {
                Controller.Stop();
                Done();
                return;
            }
        }
        else
        {
            KillTime = 0;
        }

        PrevReward = Reward;
        LastPosition = transform.position;

        */
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

