using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("All character's sprites")]
    private Sprite[] sprites;

    [SerializeField]
    private GameObject peoplePrefab;

    private List<Vector2Int[]> patterns;

    // Keep track of the people on the plateform and in the train
    private int[,] plateform, train;

    // To keep track of the number of group that were spawned, mostly for debug purpose
    private int groupNb;

    private void Start()
    {
        patterns = new List<Vector2Int[]>();
        patterns.Add(new[] { Vector2Int.zero }); // 1x1
        patterns.Add(new[] { Vector2Int.zero, Vector2Int.right, Vector2Int.up, Vector2Int.one }); // 2x2
        patterns.Add(new[] { Vector2Int.zero, Vector2Int.right, Vector2Int.right * 2, Vector2Int.right * 3 }); // 4x1
        patterns.Add(new[] { Vector2Int.zero, Vector2Int.right, Vector2Int.right * 2 }); // 2x1

        plateform = new int[5, 10];
        train = new int[3, 10];

        groupNb = 0;

        StartCoroutine(AddPeopleOnPlateform());
    }

    private IEnumerator AddPeopleOnPlateform()
    {
        while (true)
        {
            Sprite sprite = sprites[Random.Range(0, sprites.Length)];
            Vector2Int[] pattern = patterns[Random.Range(0, patterns.Count)];
            GameObject group = new GameObject("Group " + groupNb, typeof(PeopleGroup));
            foreach (Vector2Int pos in pattern)
            {
                GameObject go = Instantiate(peoplePrefab, group.transform);
                go.transform.position = (Vector2)pos;
                go.GetComponent<SpriteRenderer>().sprite = sprite;
            }

            groupNb++;
            yield return new WaitForSeconds(2f);
        }
    }
}
