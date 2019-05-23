using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SokobanAnimator))]
[RequireComponent(typeof(Animator))]
public class SokobanCharacter : MonoBehaviour
{
    // Animetor 移動速度を設定する値名
    [SerializeField] private string moveSpeedValueName = "MoveSpeed";
    private string MoveSpeedValueName { get { return moveSpeedValueName; } }

    // Animator 接地状態を設定する値名
    [SerializeField] private string groundedBoolName = "Grounded";
    private string GroundedBoolName { get { return groundedBoolName; } }

    public bool InAction { get; private set; }
    public bool IsIdle { get; private set; }

    private Animator CharacterAnimator { get; set; }
    private SokobanAnimator MoveAnimator { get; set; }

    /// <summary>
    /// 開始処理
    /// </summary>
    private void Start() {
        CharacterAnimator = GetComponent<Animator>();
        CharacterAnimator.SetBool(GroundedBoolName, true);
        MoveAnimator = GetComponent<SokobanAnimator>();
    }

    /// <summary>
    /// 回転設定
    /// </summary>
    /// <param name="angle">設定する角度</param>
    public void SetRotate(float angle) {
        transform.localEulerAngles = new Vector3(0, angle, 0);
    }

    /// <summary>
    /// 指定された時間で回転する
    /// </summary>
    /// <param name="angle">設定する角度</param>
    /// <param name="wait">アニメーション時間</param>
    public void Rotate(float angle, float wait) {
        InAction = true;
        IsIdle = false;

        CharacterAnimator.SetFloat(MoveSpeedValueName, 0.5f);
        MoveAnimator.Rotate(transform.eulerAngles.y + angle, wait, () => {
            InAction = false;
            StartCoroutine(IdleCoroutine());
        });
    }

    /// <summary>
    /// 指定された距離を進む
    /// </summary>
    /// <param name="distance">移動する距離</param>
    /// <param name="wait">アニメーション時間</param>
    public void Move(float distance, float wait) {
        InAction = true;
        IsIdle = false;

        CharacterAnimator.SetFloat(MoveSpeedValueName, 2.0f);
        MoveAnimator.MoveForward(distance, wait, () => {
            InAction = false;
            StartCoroutine(IdleCoroutine());
        });
    }

    /// <summary>
    /// 待機用コルーチン
    /// </summary>
    /// <returns>IEnumeratorインターフェイス</returns>
    private IEnumerator IdleCoroutine() {
        yield return null;

        if(!InAction && !IsIdle) {
            IsIdle = true;
            CharacterAnimator.SetFloat(MoveSpeedValueName, 0.0f);
        }
    }
}
