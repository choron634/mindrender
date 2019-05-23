using UnityEngine;

[RequireComponent(typeof(SokobanCharacter))]
public class SokobanInput : MonoBehaviour
{
    [SerializeField] private float moveWait = 0.5f;
    private float MoveWait { get { return moveWait; } }

    [SerializeField] private float rotateWait = 0.2f;
    private float RotateWait { get { return rotateWait; } }

    private SokobanTable Table { get; set; }

    private SokobanCharacter Character { get; set; }

    /// <summary>
    /// 開始処理
    /// </summary>
    private void Start() {
        Character = GetComponent<SokobanCharacter>();
        Table = FindObjectOfType<SokobanTable>();
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    private void Update() {
        if(Character.InAction) {
            return;
        }

        // 前に進む
        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
            MoveCharacter(0);
        }
        // 左に進む
        else  if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
            MoveCharacter(270);
        }
        // 右に進む
        else if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
            MoveCharacter(90);
        }
        // 下に進む
        else if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
            MoveCharacter(180);
        }
    }

    /// <summary>
    /// キャラクター移動
    /// </summary>
    /// <param name="angle">移動方向</param>
    private void MoveCharacter(float angle) {
        Character.SetRotate(angle);

        var next = Character.transform.position + Character.transform.forward;

        // 移動先が壁の場合は移動不可
        if(!Table.CanMove(next)) {
            return;
        }

        // 移動先がブロックの場合は移動可能かチェックする
        if(Table.IsBlock(next)) {
            var blockNext = next + Character.transform.forward;

            // ブロックの移動先が壁の場合は移動不可
            if(!Table.CanMove(blockNext)) {
                return;
            }

            // ブロックの移動先にブロックがある場合も移動不可
            if(Table.IsBlock(blockNext)) {
                return;
            }

            // ブロック移動
            var block = Table.GetBlock(next);
            blockNext.y = block.transform.position.y;
            block.MovePosition(blockNext, MoveWait, () => {
                Table.ChangeBlock(next, blockNext);

                Debug.Log(Table.IsSuccess());
            });
        }

        Character.Move(1, MoveWait);
    }
}
