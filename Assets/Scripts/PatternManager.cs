using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PatternManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("All character's sprites")]
    private Image[] sprites;

    private List<Vector2Int[]> patterns;

    private void Start()
    {
        patterns = new List<Vector2Int[]>();
        patterns.Add(new[] { Vector2Int.zero });
        patterns.Add(new[] { Vector2Int.zero, Vector2Int.right, Vector2Int.up, Vector2Int.one });
        patterns.Add(new[] { Vector2Int.zero, Vector2Int.up, Vector2Int.up * 2, Vector2Int.up * 3 });
        patterns.Add(new[] { Vector2Int.zero, Vector2Int.right, Vector2Int.up, Vector2Int.up * 2 });
    }
}
