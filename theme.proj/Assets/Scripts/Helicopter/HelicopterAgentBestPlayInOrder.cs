using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.UI;
using System.IO;


public class HelicopterAgentBestPlayInOrder : Agent
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
    private int ExGoal { get; set; }
    private int GoalCounter { get; set; }
    private int[] Points;
    private int PointNumber { get; set; }
    private float Distance_to_next_waypoint { get; set; }
    private float MaxDistance { get; set; }

    private int CollisionCount { get; set; }


    [SerializeField] private Text statusText = null;
    private Text StatusText { get { return statusText; } }

    [SerializeField] private string MainBrainFileName;
    [SerializeField] private string YawBrainFileName;


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

        PointNumber = PositionSensor.Points.Length;

        Points = Enumerable.Range(0, PointNumber).ToArray();
        //Points = Points.OrderBy(i => Guid.NewGuid()).ToArray();

        CurrentGoal = Points[0];
        GoalCounter = 0;

        Distance_to_next_waypoint = PositionSensor.GetDistance()[CurrentGoal];
        //MaxDistance = PositionSensor.GetDistance()[PointNumber-1];
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
    public override void AgentReset(bool GenerationChange) {
        Controller.Stop();
        PositionSensor.WaypointReset();
        transform.position = StartPosition;
        transform.rotation = StartRotation;
        LastPosition = StartPosition;
        DriveTime = 0;
        KillTime = 0;
        StopTime = 0;
        SetReward(0);

        
        //Points = Points.OrderBy(i => Guid.NewGuid()).ToArray();

        CurrentGoal = Points[0];
        GoalCounter = 0;
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
        if (distance[CurrentGoal] < 25.0f)
        {
            return true;
        }
        else
            return false;
    }

    public override List<float> CollectYawObservations()
    {
        var yawobservations = new List<float>();


        //Y軸周りの角速度
        yawobservations.Add(HeliRb.angularVelocity.y);

        // ウェイポイントまでの水平角度を取得
        yawobservations.Add(PositionSensor.GetHorizontalAngles()[CurrentGoal]);

        return yawobservations;
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
       // observations.Add(HeliRb.transform.eulerAngles.x);

        //observations.Add(HeliRb.transform.eulerAngles.y);

        //observations.Add(HeliRb.transform.eulerAngles.z);
        
        //各軸周りの角速度
        observations.Add(HeliRb.angularVelocity.x);

        //observations.Add(HeliRb.angularVelocity.y);

        observations.Add(HeliRb.angularVelocity.z);


        // ウェイポイントまでの水平角度を取得
        //observations.Add(PositionSensor.GetHorizontalAngles()[CurrentGoal]);

        return observations;
    }

    private void FixedUpdate()
    {
        DriveTime += Time.fixedDeltaTime;
        //Controller.IsOnGround = false;

        var mainobsevation = CollectObservations();
        var yawobservation = CollectYawObservations();

        var bestmainbrain = new NN(0, 0, 0, 0);
        var bestyawbrain = new NN(0, 0, 0, 0);

        bestmainbrain = MakeNN(bestmainbrain, MainBrainFileName);
        bestyawbrain = MakeNN(bestyawbrain, YawBrainFileName);

        var mainactions = bestmainbrain.Predict(mainobsevation.ToArray());
        var yawactions = bestyawbrain.Predict(yawobservation.ToArray());

        Controller.Move(Mathf.Clamp(mainactions[0], -1, 1), Mathf.Clamp(mainactions[1], -1, 1), Mathf.Clamp(mainactions[2], -1, 1), Mathf.Clamp(yawactions[0], -1, 1));

        if (ReachWaypoint())
        {
            if(GoalCounter == PointNumber - 1)
            {
                Debug.Log("success!");
                Controller.Stop();
                Done();
            }
            else
            {
                ExGoal = CurrentGoal;
                CurrentGoal = Points[GoalCounter + 1];
                GoalCounter++;
                Distance_to_next_waypoint = PositionSensor.GetDistance()[CurrentGoal];
                Controller.EngineForce = Controller.InitEngineForce;
                transform.rotation = StartRotation;
            }

        }

        if (CurrentGoal < PointNumber)
        {
            sphere = GameObject.Find("Sphere" + CurrentGoal);
            sphere.GetComponent<Renderer>().material.color = Color.red;

        }

        Vector3 CurrentPosition = HeliRb.transform.position;
        float Velocity = HeliRb.velocity.magnitude;
        float waypointdistance = PositionSensor.GetDistance()[CurrentGoal];
        int waypointobject = PositionSensor.GetObjects()[CurrentGoal];
        float horizontalangle = PositionSensor.GetHorizontalAngles()[CurrentGoal];
        float normalizeddistance = Mathf.Clamp(((Distance_to_next_waypoint - PositionSensor.GetDistance()[CurrentGoal]) / Distance_to_next_waypoint), 0, 1);
        
        if (StatusText != null)
        {
            StatusText.text = "EngineForce : " + Controller.EngineForce + "\nTail : " + Controller.Torque + "\nReward : " + Reward + "\nTime : " + DriveTime + "\nCurrentGoal : " + CurrentGoal + "\nGoalCounter : " + GoalCounter; 
        }

        if (CurrentPosition.y > 160.0f)//高く上がりすぎるものにペナルティ
        {
            //Debug.Log("too heigh");
            AddReward(-0.05f);
        }
        
        if (Velocity < 5.0f)//動かないものにペナルティ
        {
            StopTime += Time.fixedDeltaTime;
            if (StopTime > 4)
            {
                //Debug.Log("Don't move");
                AddReward(-0.05f);
            }
        }

        if (transform.position.y <= 3.0f)//地面に接触し続けるものを消す
        {
            AddReward(-0.05f);
        }

        if (Controller.IsOnGround)
        {
            AddReward(-0.05f);
        }

        if (Controller.EngineForce < 0.1)
        {
            AddReward(-0.05f);
        }


        float perpendiculardistance = GetPerpendicularDistance();
        var w = Mathf.Pow(1 - Mathf.Min(perpendiculardistance / 100, 1), 3);
        var deltaz = Mathf.Pow(1 - Mathf.Min((Math.Abs(PositionSensor.GetVector()[CurrentGoal].z) / 100), 1), 3);
        var d = 3 * Mathf.Pow(1 - Mathf.Min(PositionSensor.GetDistance()[CurrentGoal] / Distance_to_next_waypoint, 1), 3);
        var angle = Mathf.Pow((1 - Math.Abs(horizontalangle)), 2);

        AddReward(Mathf.Max((deltaz * d * angle), 0));


        if (DriveTime > 300)
        {
            Controller.Stop();
            Done();
            AgentReset(false);
            return;
        }
    }
    
    public NN MakeNN(NN bestbrain, string filename)
    {
        FileStream fs = new FileStream(filename, FileMode.Open);
        var sr = new StreamReader(fs, Encoding.UTF8);

        try
        {
            var inputsize = int.Parse(sr.ReadLine());
            var hiddensize1 = int.Parse(sr.ReadLine());
            var hiddensize2 = int.Parse(sr.ReadLine());
            var outputsize = int.Parse(sr.ReadLine());

            bestbrain = new NN(inputsize, hiddensize1, hiddensize2, outputsize);

            var inputbias = bestbrain.InputBias;
            var inputweights = bestbrain.InputWeights;
            var hiddenbias1 = bestbrain.HiddenBias1;
            var hiddenbias2 = bestbrain.HiddenBias2;
            var hiddenweights1 = bestbrain.HiddenWeights1;
            var hiddenweights2 = bestbrain.HiddenWeights2;


            for (int c = 0; c < inputbias.Colmun; c++)
            {
                inputbias[0, c] = float.Parse(sr.ReadLine());
            }
            for (int r = 0; r < inputweights.Row; r++)
            {
                for (int c = 0; c < inputweights.Colmun; c++)
                {
                    inputweights[r, c] = float.Parse(sr.ReadLine());
                }
            }
            for (int c = 0; c < hiddenbias1.Colmun; c++)
            {
                hiddenbias1[0, c] = float.Parse(sr.ReadLine());

            }
            for (int r = 0; r < hiddenweights1.Row; r++)
            {
                for (int c = 0; c < hiddenweights1.Colmun; c++)
                {
                    hiddenweights1[r, c] = float.Parse(sr.ReadLine());

                }
            }

            for (int c = 0; c < hiddenbias2.Colmun; c++)
            {
                hiddenbias2[0, c] = float.Parse(sr.ReadLine());

            }
            for (int r = 0; r < hiddenweights2.Row; r++)
            {
                for (int c = 0; c < hiddenweights2.Colmun; c++)
                {
                    hiddenweights2[r, c] = float.Parse(sr.ReadLine());

                }
            }

        }
        finally
        {
            sr.Close();
        }
        return bestbrain;

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
            AddReward(-10);

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
            a = PositionSensor.GetPosition()[ExGoal];
        }
        Vector3 b = PositionSensor.GetPosition()[CurrentGoal];
        Vector3 CurrentPosition = HeliRb.transform.position;
        var h = GetPerpendicularFootPrint(a, b, CurrentPosition);
        float distance = Vector3.Distance(CurrentPosition, h);
        return distance;
    }
}

