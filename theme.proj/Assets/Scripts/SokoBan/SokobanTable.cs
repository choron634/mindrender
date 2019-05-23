using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SokobanTable : MonoBehaviour
{
    [SerializeField] private Vector2Int size = Vector2Int.zero;
    private Vector2Int Size { get { return size; } }

    [SerializeField] private GameObject walls = null;
    private GameObject Walls { get { return walls; } }

    [SerializeField] private GameObject blocks = null;
    private GameObject Blocks { get { return blocks; } }

    [SerializeField] private GameObject goals = null;
    private GameObject Goals { get { return goals; } }

    private int[,] WallTable { get; set; }
    private int[,] BlockTable { get; set; }

    private List<SokobanAnimator> BlockRefs { get; set; } = new List<SokobanAnimator>();

    private SokobanAnimator[] Characters { get; set; }

    /// <summary>
    /// 開始処理
    /// </summary>
    private void Start() {
        // テーブル初期化
        WallTable = new int[Size.y, Size.x];
        BlockTable = new int[Size.y, Size.x];

        // 壁設定
        SetTable(WallTable, Walls, 1);

        // ゴール設定
        SetTable(WallTable, Goals, 2);

        // ブロック設定
        SetTable(BlockTable, Blocks, 1);

        // ブロックの参照を取得
        BlockRefs.AddRange(Blocks.GetComponentsInChildren<SokobanAnimator>());

        // リセット用に動く対象物の参照を取得
        Characters = FindObjectsOfType<SokobanAnimator>();
    }

    /// <summary>
    /// 指定された移動可能か取得
    /// </summary>
    /// <param name="position">指定座標</param>
    /// <returns>指定された座標が移動可能な場合はtrue</returns>
    public bool CanMove(Vector3 position) {
        var (x, y) = PositionToCell(position);

        // 範囲外は移動不可
        if(!IsRange(x, y)) {
            return false;
        }

        return WallTable[y, x] != 1;
    }

    /// <summary>
    /// 指定された座標にブロックがあるか取得
    /// </summary>
    /// <param name="position">指定座標</param>
    /// <returns>指定された座標にブロックが存在する場合はtrue</returns>
    public bool IsBlock(Vector3 position) {
        var (x, y) = PositionToCell(position);
        return BlockTable[y, x] != 0;
    }

    /// <summary>
    /// 指定された座標のブロックを取得
    /// </summary>
    /// <param name="position">指定座標</param>
    /// <returns>指定された座標にブロックが存在する場合はブロックの参照を返す</returns>
    public SokobanAnimator GetBlock(Vector3 position) {
        var (x, y) = PositionToCell(position);
        return BlockRefs.FirstOrDefault(b => {
            var (bx, by) = PositionToCell(b.transform.position);
            return bx == x && by == y;
        });
    }

    /// <summary>
    /// ブロックテーブルを入れ替える
    /// </summary>
    /// <param name="prev">入れ替え元の座標</param>
    /// <param name="next">入れ替え先の座標</param>
    public void ChangeBlock(Vector3 prev, Vector3 next) {
        var (px, py) = PositionToCell(prev);
        var (nx, ny) = PositionToCell(next);
        BlockTable[py, px] = 0;
        BlockTable[ny, nx] = 1;
    }

    /// <summary>
    /// 全てのブロックがゴール地点に置かれているか取得
    /// </summary>
    /// <returns>全てゴール地点に置かれている場合はtrue</returns>
    public bool IsSuccess() {
        return BlockRefs.Where(b => {
            var (x, y) = PositionToCell(b.transform.position);
            return WallTable[y, x] == 2;
        }).Count() == BlockRefs.Count;
    }

    /// <summary>
    /// 全ての状態をリセットする
    /// </summary>
    public void ResetTable() {
        Array.ForEach(Characters, c => c.ResetPosition());

        Array.Clear(BlockTable, 0, BlockTable.Length);
        SetTable(BlockTable, Blocks, 1);
    }

    /// <summary>
    /// テーブルを設定
    /// </summary>
    /// <param name="table">設定するテーブル</param>
    /// <param name="parent">配置するオブジェクトの親オブジェクト</param>
    /// <param name="value">設定する値</param>
    private void SetTable(int[,] table, GameObject parent, int value) {
        for(int i = 0; i < parent.transform.childCount; i++) {
            var child = parent.transform.GetChild(i);
            var (x, y) = PositionToCell(child.transform.position);
            table[y, x] = value;
        }
    }

    /// <summary>
    /// 指定された座標が範囲内にあるか取得
    /// </summary>
    /// <param name="x">指定 x 座標</param>
    /// <param name="y">指定 y 座標</param>
    /// <returns>範囲内の場合はtrue</returns>
    private bool IsRange(int x, int y) {
        if(x < 0 || y < 0 || x >= WallTable.GetLength(1) || y >= WallTable.GetLength(0)) {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 指定された座標をセル座標に変換する
    /// </summary>
    /// <param name="position">指定座標</param>
    /// <returns>変換されたセル座標</returns>
    private (int, int) PositionToCell(Vector3 position) {
        var y = Mathf.FloorToInt(position.z);
        var x = Mathf.FloorToInt(position.x);
        return (x, y);
    }
}
