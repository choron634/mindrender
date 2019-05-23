using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PuzzlePanel : MonoBehaviour
{
    public int Index { get; set; }

    public Vector2Int Position { get; set; } = Vector2Int.zero;
}
