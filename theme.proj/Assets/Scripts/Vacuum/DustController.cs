using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DustController : MonoBehaviour
{
    private List<GameObject> Dusts { get; set; } = new List<GameObject>();

    /// <summary>
    /// 開始時に呼び出される処理
    /// </summary>
    private void Start() {
        for(int i = 0; i < transform.childCount; i++) {
            Dusts.Add(transform.GetChild(i).gameObject);
        }
    }

    public void ResetDust() {
        Dusts.ForEach(o => o.SetActive(true));
    }
}
