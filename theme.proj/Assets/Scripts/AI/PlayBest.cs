using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.UI;
using System.IO;

public class PlayBest : Agent
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

    NN bestbrain { get; set; } = new NN(0,0,0,0);


    /// <summary>
    /// 開始時に呼び出される初期化処理
    /// </summary>
    private void Start()
    {
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
        MaxDistance = PositionSensor.GetDistance()[PointNumber - 1];
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
    public new void AgentReset()
    {
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

    public new List<float> CollectObservations()
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

    public void LoadBest()
    {
        bestbrain.Load("savedata.txt");
    }

    public void MakeNN()
    {
        FileStream fs = new FileStream("bestbrain.txt", FileMode.Open);
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

    }

    private void FixedUpdate()
    {
        var observations = CollectObservations();

        LoadBest();

        var actions = bestbrain.Predict(observations.ToArray());

        Controller.Move(Mathf.Clamp(actions[0], -1, 1), Mathf.Clamp(actions[1], -1, 1), Mathf.Clamp(actions[2], -1, 1), Mathf.Clamp(actions[3], -1, 1));

        if (StatusText != null)
        {
            StatusText.text = "EngineForce : " + Controller.EngineForce + "\nTorque : " + Controller.Torque + "\nReward : " + Reward + "\nTime : " + DriveTime;
        }

        DriveTime += Time.fixedDeltaTime;
    }

    public void OnCollisionEnter(Collision collision)
    {

        // 例えばtagがwallだったらの判定
        if (collision.gameObject.tag == "wall")
        {
            Controller.Stop();
            Done();
            AgentReset();
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        other.gameObject.SetActive(false);
    }

    private float Clamp(float v, float min, float max)
    {
        return min + (max - min) * (v + 1.0f) / 2.0f;
    }

}

