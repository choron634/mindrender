using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RCC_CarControllerV3))]
public class CarAgent : MonoBehaviour
{
    private Sensor[] Sensors { get; set; }

    private RCC_CarControllerV3 Controller { get; set; }

    /// <summary>
    /// 開始時に呼び出される初期化処理
    /// </summary>
    private void Start() {
        // 車の制御コントローラーを取得
        Controller = GetComponent<RCC_CarControllerV3>();

        // 子に追加されているセンサーを全て取得
        Sensors = GetComponentsInChildren<Sensor>();

        // エンジンを起動
        Controller.StartEngine();
    }

    /// <summary>
    /// 毎フレーム呼び出される処理
    /// Updateではなくこちらを使用する
    /// </summary>
    public void FixedUpdate() {
        // 速度設定(アクセル)
        Controller.gasInput = 0.0f;

        // ハンドル操作
        Controller.steerInput = 0.0f;

        // ブレーキ操作
        Controller.brakeInput = 0.0f;

        // センサーの距離をリストに追加する
        var results = new List<float>();
        Array.ForEach(Sensors, sensor => {
            results.AddRange(sensor.Hits());
        });
    }

    /// <summary>
    /// 衝突時に呼び出されるコールバック
    /// </summary>
    /// <param name="collision"></param>
    public void OnCollisionEnter(Collision collision) {
        // 例えばtagがwallだったらの判定
        if(collision.gameObject.tag == "wall") {
            // エンジンを停止
            Controller.KillEngine();
        }
    }
}
