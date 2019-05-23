using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PuzzleController : MonoBehaviour
{
    [SerializeField] private RectTransform emptyPanel = null;
    private RectTransform EmptyPanel { get { return emptyPanel; } }

    [SerializeField] private List<Vector2Int> directions = new List<Vector2Int>();
    private List<Vector2Int> Directions { get { return directions; } }

    [SerializeField] private int panelSize = 4;
    private int PanelSize { get { return panelSize; } }

    private int[,] AnswerTable { get; set; }
    private int[,] PanelTable { get; set; }

    private PuzzlePanel[] Panels { get; set; }

    private Vector2Int EmptyPosition { get; set; }

    private List<Vector2> DefaultPositions { get; set; } = new List<Vector2>();
    private Vector2 DefaultEmptyPosition { get; set; }

    /// <summary>
    /// 開始処理
    /// </summary>
    private void Start() {
        DefaultEmptyPosition = EmptyPanel.anchoredPosition;
        Panels = GetComponentsInChildren<PuzzlePanel>();

        var index = 1;
        Array.ForEach(Panels, panel => {
            DefaultPositions.Add(panel.GetComponent<RectTransform>().anchoredPosition);
            panel.Index = index;
            panel.GetComponent<Button>().onClick.AddListener(() => Replace(panel));
            index++;
        });

        AnswerTable = new int[PanelSize, PanelSize];
        PanelTable = new int[PanelSize, PanelSize];

        ResetPanel();
    }

    /// <summary>
    /// 空白パネルを入れ替える
    /// </summary>
    /// <param name="panel">入れ替え元のパネル</param>
    private void Replace(PuzzlePanel panel) {
        for(int i = 0; i < Directions.Count; i++) {
            var newPosition = Directions[i] + panel.Position;
            if(newPosition.x < 0 || newPosition.y < 0 || newPosition.x >= PanelSize || newPosition.y >= PanelSize) {
                continue;
            }

            if(PanelTable[newPosition.y, newPosition.x] == 0) {
                ReplacePanel(panel, newPosition, 0.15f);
                break;
            }
        }
    }

    /// <summary>
    /// パネルを移動する
    /// </summary>
    /// <param name="panel">移動するパネル</param>
    /// <param name="position">移動先のセル座標</param>
    /// <param name="wait">アニメーション時間</param>
    private void ReplacePanel(PuzzlePanel panel, Vector2Int position, float wait = 0.0f) {
        var temp = EmptyPanel.anchoredPosition;
        EmptyPanel.anchoredPosition = panel.GetComponent<RectTransform>().anchoredPosition;
        PanelTable[position.y, position.x] = panel.Index;
        PanelTable[panel.Position.y, panel.Position.x] = 0;
        EmptyPosition = panel.Position;
        panel.Position = position;

        if(wait > 0) {
            StartCoroutine(MoveCoroutine(panel.GetComponent<RectTransform>(), temp, wait));
        }
        else {
            panel.GetComponent<RectTransform>().anchoredPosition = temp;
        }
    }

    /// <summary>
    /// 移動用コルーチン
    /// </summary>
    /// <param name="rectTrans">移動させるRectTransform</param>
    /// <param name="position">移動先の座標</param>
    /// <param name="wait">アニメーション時間</param>
    /// <returns></returns>
    private IEnumerator MoveCoroutine(RectTransform rectTrans, Vector2 position, float wait) {
        var current = EventSystem.current;
        current.enabled = false;

        var now = 0.0f;
        var org = rectTrans.anchoredPosition;
        while(now < wait) {
            now += Time.deltaTime;
            rectTrans.anchoredPosition = Vector2.Lerp(org, position, now / wait);
            yield return 0;
        }

        current.enabled = true;
    }

    /// <summary>
    /// パネルを指定回数入れ替える
    /// </summary>
    /// <param name="count">入れ替える回数</param>
    public void Shuffle(int count) {
        var dir = Vector2Int.zero;
        for(int i = 0; i < count; i++) {
            var filter = Directions.Where(d => {
                if(d == (dir * -1)) {
                    return false;
                }

                var newPosition = d + EmptyPosition;
                if(newPosition.x < 0 || newPosition.y < 0 || newPosition.x >= PanelSize || newPosition.y >= PanelSize) {
                    return false;
                }

                return true;
            }).ToList();

            var index = UnityEngine.Random.Range(0, filter.Count);
            var replacePosition = EmptyPosition + filter[index];
            var panel = Panels.First(p => p.Position == replacePosition);
            ReplacePanel(panel, EmptyPosition);

            dir = filter[index];
            //Debug.Log(dir);
            Debug.Log(panel.Index);
        }
    }

    /// <summary>
    /// 全てのパネルを初期状態にする
    /// </summary>
    public void ResetPanel() {
        EmptyPosition = new Vector2Int(PanelSize - 1, PanelSize - 1);
        EmptyPanel.anchoredPosition = DefaultEmptyPosition;

        Array.Sort(Panels, (p1, p2) => p1.Index - p2.Index);
        Array.Clear(AnswerTable, 0, AnswerTable.Length);
        Array.Clear(PanelTable, 0, PanelTable.Length);
        for(int i = 0; i < Panels.Length; i++) {
            var y = (i / PanelSize);
            var x = (i % PanelSize);

            var panel = Panels[i];
            panel.Position = new Vector2Int(x, y);
            panel.GetComponent<RectTransform>().anchoredPosition = DefaultPositions[i];
            AnswerTable[y, x] = (i + 1);
            PanelTable[y, x] = (i + 1);
        }
    }
}
