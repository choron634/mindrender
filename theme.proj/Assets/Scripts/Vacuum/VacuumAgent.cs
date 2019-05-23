using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VacuumAgent : MonoBehaviour
{
    private Sensor[] Sensors { get; set; }

    private Rigidbody VacuumRb { get; set; }

    private float Score { get; set; }

    private Vector3 StartPosition { get; set; }
    private Quaternion StartRotation { get; set; }

    public bool IsDead { get; private set; }

    /// <summary>
    /// 開始時に呼び出される初期化処理
    /// </summary>
    private void Start() {
        // Rigidbody取得
        VacuumRb = GetComponent<Rigidbody>();

        // センサー取得
        Sensors = GetComponentsInChildren<Sensor>();

        StartPosition = transform.position;
        StartRotation = transform.rotation;

        // 20秒で90度回転させる
        Rotation(90, 20);

        // 速度1で常に前進させる
        Forward(1);
    }

    /// <summary>
    /// 毎フレーム呼び出される処理
    /// Updateではなくこちらを使用する
    /// </summary>
    public void FixedUpdate() {

        // センサーの距離をリストに追加する
        var results = new List<float>();
        Array.ForEach(Sensors, sensor => {
            results.AddRange(sensor.Hits());
        });
    }

    public void OnCollisionEnter(Collision collision) {
        // 壁に当たったときの処理
        if(collision.gameObject.tag == "wall") {
        }
    }

    public void OnTriggerEnter(Collider other) {
        // ゴミに接触したときの処理
        other.gameObject.SetActive(false);
        Score += 0.5f;
    }

    public void ResetVacuum() {
        // 座標、回転を戻す
        transform.position = StartPosition;
        transform.rotation = StartRotation;

        // 移動停止
        StopAllCoroutines();

        // スコア初期化
        Score = 0;
    }

    /// <summary>
    /// 回転処理
    /// </summary>
    /// <param name="angle">回転させる角度</param>
    /// <param name="time">回転にかかる時間</param>
    private void Rotation(float angle, float time) {
        StopRotation();
        StartCoroutine(RotationCoroutine(angle, time));
    }

    /// <summary>
    /// 前進
    /// </summary>
    /// <param name="speed">速度</param>
    private void Forward(float speed) {
        StopForward();
        StartCoroutine(ForwardCoroutine(speed));
    }

    /// <summary>
    /// 前進停止
    /// </summary>
    private void StopForward() {
        StopCoroutine("ForwardCoroutine");
    }

    /// <summary>
    /// 回転停止
    /// </summary>
    private void StopRotation() {
        StopCoroutine("RotationCoroutine");
    }

    /// <summary>
    /// 前進、回転停止
    /// </summary>
    private void Stop() {
        StopForward();
        StopRotation();
    }

    private IEnumerator RotationCoroutine(float angle, float time) {
        var rotation = VacuumRb.rotation;
        var nextRotation = Quaternion.AngleAxis(angle, transform.up);

        var now = 0.0f;
        while(now < time) {
            VacuumRb.rotation = Quaternion.Lerp(rotation, nextRotation, now / time);
            now += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator ForwardCoroutine(float speed) {
        while(true) {
            VacuumRb.velocity = transform.forward * speed;
            yield return new WaitForFixedUpdate();
        }
    }
}
