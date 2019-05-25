using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.UI;

public class HelicopterAgent : Agent
{

    private Sensor[] Sensors { get; set; }

    private WaypointSensor PositionSensor { get; set; }

    private SimpleHelicopterController Controller { get; set; }

    private Vector3 StartPosition { get; set; }
    private Quaternion StartRotation { get; set; }

    private Vector3 LastPosition { get; set; }

    public float DriveTime { get; set; }
    private float PrevReward { get; set; }
    private float KillTime { get; set; }
    private float StopTime { get; set; }


    private Rigidbody HeliRb { get; set; }
    private RayPerception RayPer { get; set; }

    private int CurrentGoal { get; set; }
    private int PointNumber { get; set; }
    private float Distance_to_next_waypoint { get; set; }
    private float MaxDistance { get; set; }

    private int CollisionCount { get; set; }


    [SerializeField] private Text statusText = null;
    private Text StatusText { get { return statusText; } }

    GameObject sphere;


    /// <summary>
    /// 開始時に呼び出される初期化処理
    /// </summary>
    private void Start() {
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
        CurrentGoal = 0;
        PointNumber = PositionSensor.Points.Length;
        Distance_to_next_waypoint = PositionSensor.GetDistance()[CurrentGoal];
        MaxDistance = PositionSensor.GetDistance()[PointNumber-1];
        CollisionCount = 0;
        
        
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
        Controller.Stop();
        PositionSensor.WaypointReset();
        transform.position = StartPosition;
        transform.rotation = StartRotation;
        LastPosition = StartPosition;
        DriveTime = 0;
        KillTime = 0;
        StopTime = 0;
        SetReward(0);
        CurrentGoal = 0;
        Distance_to_next_waypoint = PositionSensor.GetDistance()[CurrentGoal];
        for (int i = 0; i < PointNumber; i++)
        {
            sphere = GameObject.Find("Sphere" + i);
            sphere.GetComponent<Renderer>().material.color = Color.green;
        }
        CollisionCount = 0;
    }

    public bool ReachWaypoint()
    {
        var distance = PositionSensor.GetDistance();
        if (distance[CurrentGoal] < 10.0f)
        {
            AddReward(1000);
            return true;
        }
        else
            return false;
    }

    public override List<float> CollectObservations()
    {
        var observations = new List<float>();

        var v = HeliRb.velocity;

        //Debug.Log("velocity" + v);

        //Debug.Log(Sensors[0]);

        // センサーの距離をリストに追加する
        Array.ForEach(Sensors, sensor => {
            observations.AddRange(sensor.Hits());
        });

        //
        observations.Add(v.x);

        observations.Add(v.y);

        observations.Add(v.z);

        // ウェイポイントまでの障害物数を取得
        //observations.Add(PositionSensor.GetObjects()[CurrentGoal]);

        //回る順番を決めなきゃいけない。

        // ウェイポイントまでの直線距離,ベクトルを取得
        //observations.Add(PositionSensor.GetDistance()[CurrentGoal]);

        observations.Add(PositionSensor.GetVector()[CurrentGoal].x);

        observations.Add(PositionSensor.GetVector()[CurrentGoal].y);

        observations.Add(PositionSensor.GetVector()[CurrentGoal].z);

        //各軸周りの回転角
        observations.Add(HeliRb.transform.eulerAngles.x);

        observations.Add(HeliRb.transform.eulerAngles.y);

        observations.Add(HeliRb.transform.eulerAngles.z);
        
        //各軸周りの角速度
        observations.Add(HeliRb.angularVelocity.x);

        observations.Add(HeliRb.angularVelocity.y);

        observations.Add(HeliRb.angularVelocity.z);


        // ウェイポイントまでの水平角度を取得
        //observations.Add(PositionSensor.GetHorizontalAngles()[CurrentGoal]);

        return observations;
    }

    public override void AgentAction(float[] outputs)
    {
        Controller.Move(Mathf.Clamp(outputs[0],-1,1), Mathf.Clamp(outputs[1], -1,1), Mathf.Clamp(outputs[2],-1,1), Mathf.Clamp(outputs[3], -1, 1));

        if (ReachWaypoint())
        {
            if (CurrentGoal < PointNumber - 1)
            {
                sphere = GameObject.Find("Sphere"+CurrentGoal);
                sphere.GetComponent<Renderer>().material.color = Color.red;
                CurrentGoal++;
                Distance_to_next_waypoint = PositionSensor.GetDistance()[CurrentGoal];
            }
        }

        float x = outputs[0];
        float y = outputs[1];
        float z = outputs[2];
        float waypointdistance = PositionSensor.GetDistance()[CurrentGoal];
        int waypointobject = PositionSensor.GetObjects()[CurrentGoal];
        float horizontalangle = PositionSensor.GetHorizontalAngles()[CurrentGoal];
        float normalizeddistance = Mathf.Clamp(((Distance_to_next_waypoint - PositionSensor.GetDistance()[CurrentGoal])/ Distance_to_next_waypoint),0,1);

        //AddReward((1.0f - Mathf.Abs(PositionSensor.GetHorizontalAngles()[CurrentGoal]))*3);
        //AddReward(Mathf.Clamp(((Distance_to_next_waypoint - PositionSensor.GetDistance()[CurrentGoal])/Distance_to_next_waypoint),0,1)*2);
       

        if (StatusText != null)
        {
            StatusText.text = "EngineForce : " + Controller.EngineForce + "\nTorque : " + Controller.Torque + "\nReward : " + Reward + "\nTime : " + DriveTime;
        }

        DriveTime += Time.fixedDeltaTime;

        //Debug.Log(DriveTime);


        if (transform.position.y <= 1.0f)//地面に接触し続けるものにペナルティ
        {
            if (DriveTime >= 3.0f)
            {
                //Debug.Log("too low");
                AddReward(-0.1f);
            }
        }
        /*
                if (transform.position.y > 93.0f)//高く上がりすぎるものにペナルティ
                {
                    if (DriveTime >= 3.0f)
                    {
                        Debug.Log("too heigh");
                        AddReward(-0.1f);
                    }
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
        /* if ((x * x + y * y + z * z) < 1.0f)//動かないものにペナルティ
         {
             StopTime += Time.fixedDeltaTime;
             if (StopTime > 4)
             {
                 Debug.Log("Don't move");
                 AddReward(-0.5f);
             }
         }
        */

        float perpendiculardistance = GetPerpendicularDistance();
        var w = 1 - Mathf.Pow(Mathf.Min(perpendiculardistance/100 ,1),3);
        var deltaz = 1- Mathf.Pow(Mathf.Min((Math.Abs(PositionSensor.GetVector()[CurrentGoal].z)/100),1),3);
        var d = 1 - Mathf.Pow(Mathf.Min(PositionSensor.GetDistance()[CurrentGoal]/Distance_to_next_waypoint,1), 3);

        AddReward(Mathf.Max((w * 2 * (CurrentGoal + 1) * d + 2*deltaz - Math.Abs(horizontalangle) - CollisionCount/20),0));

       if (DriveTime > (CurrentGoal + 1)*9)//段階的に時間を延ばす
        {
            //AddReward(Mathf.Clamp((MaxDistance - PositionSensor.GetDistance()[PointNumber - 1]), 0, MaxDistance) * 10);
           // Debug.Log("Done!");
            Controller.Stop();
            Done();
            //Reward = Reward / DriveTime;
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

        if(transform.position.y <= 3.0f)//地面に接触し続けるものを消す
        {
            KillTime += Time.fixedDeltaTime;
            if(KillTime >= 5.0f)
            {
                Controller.Stop();
                AddReward(-10);
                Done();
                return;
            }
        }
        

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
        if (CurrentGoal == 0)
        {
            a = StartPosition;
        }
        else
        {
            a = PositionSensor.GetPosition()[CurrentGoal - 1];
        }
        Vector3 b = PositionSensor.GetPosition()[CurrentGoal];
        Vector3 CurrentPosition = HeliRb.transform.position;
        var h = GetPerpendicularFootPrint(a, b, CurrentPosition);
        float distance = Vector3.Distance(CurrentPosition, h);
        return distance;
    }
}

