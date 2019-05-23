using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class SokobanAnimator : MonoBehaviour
{
    private Vector3 StartPosition { get; set; }
    private Quaternion StartRotation { get; set; }

    private EventSystem CurrentSystem { get; set; }

    /// <summary>
    /// 開始処理
    /// </summary>
    private void Start() {
        CurrentSystem = EventSystem.current;
        StartPosition = transform.position;
        StartRotation = transform.rotation;
    }

    /// <summary>
    /// 座標リセット
    /// </summary>
    public void ResetPosition() {
        transform.position = StartPosition;
        transform.rotation = StartRotation;
    }

    /// <summary>
    /// 指定された時間で前進する
    /// </summary>
    /// <param name="distance">移動する距離</param>
    /// <param name="wait">アニメーション時間</param>
    /// <param name="onCompletion">移動完了時に呼び出されるコールバック</param>
    public void MoveForward(float distance, float wait, Action onCompletion = null) {
        StartCoroutine(MoveCoroutine(transform.position + transform.forward * distance, wait, onCompletion));
    }

    /// <summary>
    /// 指定された時間で座標を移動する
    /// </summary>
    /// <param name="position">移動先の座標</param>
    /// <param name="wait">アニメーション時間</param>
    /// <param name="onCompletion">移動完了時に呼び出されるコールバック</param>
    public void MovePosition(Vector3 position, float wait, Action onCompletion = null) {
        StartCoroutine(MoveCoroutine(position, wait, onCompletion));
    }

    /// <summary>
    /// 指定された時間で回転する
    /// </summary>
    /// <param name="angle">回転する角度</param>
    /// <param name="wait">アニメーション時間</param>
    /// <param name="onCompletion">回転完了時に呼び出されるコールバック</param>
    public void Rotate(float angle, float wait, Action onCompletion = null) {
        StartCoroutine(RotateCoroutine(angle, wait, onCompletion));
    }

    /// <summary>
    /// 移動用コルーチン
    /// </summary>
    /// <param name="next">移動先座標</param>
    /// <param name="wait">アニメーション時間</param>
    /// <param name="onCompletion">移動完了時に呼び出されるコールバック</param>
    /// <returns>IEnumeratorインターフェイス</returns>
    private IEnumerator MoveCoroutine(Vector3 next, float wait, Action onCompletion = null) {
        CurrentSystem.enabled = false;
        var prev = transform.position;
        var now = 0.0f;

        while(now < wait) {
            now += Time.deltaTime;
            transform.position = Vector3.Lerp(prev, next, now / wait);
            yield return 0;
        }

        transform.position = next;

        onCompletion?.Invoke();
        CurrentSystem.enabled = true;
    }

    /// <summary>
    /// 回転用コルーチン
    /// </summary>
    /// <param name="angle">回転角度</param>
    /// <param name="wait">アニメーション時間</param>
    /// <param name="onCompletion">回転完了時に呼び出されるコールバック</param>
    /// <returns>IEnumeratorインターフェイス</returns>
    private IEnumerator RotateCoroutine(float angle, float wait, Action onCompletion = null) {
        CurrentSystem.enabled = false;
        var prev = transform.eulerAngles.y;
        var next = angle;
        var now = 0.0f;

        while(now < wait) {
            now += Time.deltaTime;
            transform.eulerAngles = new Vector3(0, Mathf.Lerp(prev, next, now / wait), 0);
            yield return 0;
        }

        transform.eulerAngles = new Vector3(0, (int)transform.eulerAngles.y, 0);

        onCompletion?.Invoke();
        CurrentSystem.enabled = true;
    }
}
