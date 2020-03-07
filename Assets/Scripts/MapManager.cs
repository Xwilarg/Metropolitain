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

    [SerializeField]
    private GameObject trainTile;

    private List<Vector2Int[]> patterns;

    // Keep track of the people on the plateform and in the train
    private int[,] plateform;
    private bool[,] train;

    // To keep track of the number of group that were spawned, mostly for debug purpose
    private int groupNb;

    // Dimensions for plateform and train
    private const int plateformX = 5, plateformY = 10;
    private const int trainX = 3, trainY = 10;

    // Train position
    private const int trainTileX = -5, trainTileY = -10;

    private void Start()
    {
        patterns = new List<Vector2Int[]>();
        patterns.Add(new[] { Vector2Int.zero }); // 1x1
        patterns.Add(new[] { Vector2Int.zero, Vector2Int.right, Vector2Int.up, Vector2Int.one }); // 2x2
        patterns.Add(new[] { Vector2Int.zero, Vector2Int.right, Vector2Int.right * 2, Vector2Int.right * 3 }); // 4x1
        patterns.Add(new[] { Vector2Int.zero, Vector2Int.right, Vector2Int.right * 2 }); // 2x1

        plateform = new int[plateformX, plateformY];
        train = new bool[trainX, trainY];
        for (var i = 0; i < plateformX * plateformY; i++)
            plateform[i % plateformX, i / plateformY] = 0;
        for (var i = 0; i < trainX * trainY; i++)
            train[i % trainX, i / trainY] = true;

        groupNb = 0;

        StartCoroutine(AddPeopleOnPlateform());

        for (int x = 0; x < trainX; x++)
            for (int y = 0; y < trainY; y++)
                Instantiate(trainTile, new Vector2(x + trainTileX,y + trainTileY) , Quaternion.identity);
    }

    private IEnumerator AddPeopleOnPlateform()
    {
        while (true)
        {
            Sprite sprite = sprites[Random.Range(0, sprites.Length)];
            List<Vector2Int[]> allPatterns = new List<Vector2Int[]>(patterns);
            Vector2Int[] pattern;
            Vector2Int? posOnPlateform;
            do
            {
                if (allPatterns.Count == 0)
                    throw new System.Exception("No more space on plateform");

                pattern = allPatterns[Random.Range(0, allPatterns.Count)];
                posOnPlateform = PlaceOnPlateform(pattern);
                allPatterns.Remove(pattern);
            } while (!posOnPlateform.HasValue); // Try to find is pattern an be put on plateform

            foreach (var pos in pattern)
                plateform[pos.x, pos.y] = 1;

            GameObject group = new GameObject("Group " + groupNb, typeof(PeopleGroup));
            foreach (Vector2Int pos in pattern)
            {
                GameObject go = Instantiate(peoplePrefab, group.transform);
                go.transform.position = (Vector2)pos + posOnPlateform.Value;
                go.GetComponent<SpriteRenderer>().sprite = sprite;
            }

            groupNb++;
            yield return new WaitForSeconds(2f);
        }
    }

    /// <summary>
    /// Check if a position on the plateform is occupied or not
    /// <returns></returns>
    private bool IsPlateformPosFree(int x, int y)
        => x < plateformX && y < plateformY && plateform[x, y] == 0;

    /// <summary>
    /// Check if we can put a pattern on the y position of a plateform
    /// </summary>
    /// <returns>Return X pos of the pattern or -1 if it doesn't fit</returns>
    private int DoesPatternFitOnPlateform(int y, Vector2Int[] pattern)
    {
        int initX = -1; // X position on the plateform
        for (int x = 0; x < plateformX; x++) // We find the first free space available on the line Y
        {
            if (IsPlateformPosFree(x, y))
            {
                initX = x;
                break;
            }
        }
        if (initX == -1) // No position available for first element of pattern
            return -1;

        foreach (var elem in pattern)
        {
            if (!IsPlateformPosFree(initX + elem.x, y + elem.y))
                return -1;
        }
        return initX;
    }

    /// <summary>
    /// Attempt to place people on the plateform
    /// </summary>
    /// <returns>The position of the group on the plateform or null if they can't be placed</returns>
    private Vector2Int? PlaceOnPlateform(Vector2Int[] pattern)
    {
        List<Vector2Int> posAvailable = new List<Vector2Int>(); // All Y pos where we can put the pattern
        for (int i = 0; i < plateformY; i++) // For each plateform lines
        {
            int xPos = DoesPatternFitOnPlateform(i, pattern);
            if (xPos != -1)
                posAvailable.Add(new Vector2Int(xPos, i));
        }
        if (posAvailable.Count == 0)
            return null;
        return posAvailable[Random.Range(0, posAvailable.Count)];
    }
}
