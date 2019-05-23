using System.Collections.Generic;
using UnityEngine;

public class RayPerception : MonoBehaviour
{
    private List<float> PerceptionBuffer { get; set; } = new List<float>();

    /// <summary>
    /// 衝突テスト
    /// </summary>
    /// <param name="rayDistance">距離</param>
    /// <param name="rayAngles">角度</param>
    /// <param name="layer">衝突対象のレイヤー</param>
    /// <param name="isNormalized">正規化指定</param>
    /// <returns>角度に対してRayが衝突した距離</returns>
    public List<float> Perceive(float rayDistance, float[] rayAngles, int layer, bool isNormalized = true) {
        PerceptionBuffer.Clear();
        foreach(float angle in rayAngles) {
            var start = transform.position;
            var direction = transform.TransformDirection(PolarToCartesian(angle));
            RaycastHit hit;
            if(Physics.Raycast(start, direction, out hit, rayDistance, 1 << layer)) {
            //if(Physics.SphereCast(start, 0.5f, direction, out hit, rayDistance, 1 << layer)) {
                if(isNormalized) {
                    PerceptionBuffer.Add(hit.distance / rayDistance);
                }
                else {
                    PerceptionBuffer.Add(hit.distance);
                }

#if UNITY_EDITOR
                Debug.DrawRay(start, direction * hit.distance, new Color(1, 0, 0, 0.5f), 0.01f, true);
#endif
            }
            else {
                PerceptionBuffer.Add(isNormalized ? 1 : rayDistance);
#if UNITY_EDITOR
                Debug.DrawRay(start, direction * rayDistance, new Color(0, 1, 0, 0.5f), 0.01f, true);
#endif
            }
        }

        return PerceptionBuffer;
    }


    /// <summary>
    /// 円周上の座標を取得
    /// </summary>
    /// <param name="angle">角度</param>
    /// <returns>指定した角度に対する円周上の座標</returns>
    public static Vector3 PolarToCartesian(float angle) {
        float x = Mathf.Cos(Mathf.Deg2Rad * angle);
        float z = Mathf.Sin(Mathf.Deg2Rad * angle);
        return new Vector3(x, 0f, z);
    }
}
