using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private int[,] plateform;
    private bool[,] train;

    // To keep track of the number of group that were spawned, mostly for debug purpose
    private int groupNb;

    // Dimensions for plateform and train
    private const int plateformX = 5, plateformY = 10;
    private const int trainX = 5, trainY = 11;
    public int GetTrainX() => trainX;
    public int GetTrainY() => trainY;

    // Plateform position
    private const float plateformPosX = 0f, plateformPosY = -4.5f;

    private TrainSpot[] trainSpots = new TrainSpot[trainX * trainY];

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
        for (int x = 0; x < trainX; x++)
            for (int y = 0; y < trainY; y++)
                train[x, y] = true;

        groupNb = 0;

        StartCoroutine(AddPeopleOnPlateform());
    }

    public void SetSpot(int pos, TrainSpot spot)
        => trainSpots[pos] = spot;

    /// <summary>
    /// Check if a position is in the train and free
    /// </summary>
    public bool IsPositionOnTrain(Vector2 pos)
    {
        var spot = GetClosestSpot(pos);
        if (spot == null) // Outside of train
            return false;
        return train[spot.Position.x, spot.Position.y];
    }

    public void LockPositionOnTrain(Vector2 pos)
    {
        var spot = GetClosestSpot(pos);
        train[spot.Position.x, spot.Position.y] = false;
    }

    public Vector2 GetOffset(Vector2 pos)
        => (Vector2)GetClosestSpot(pos).transform.position - pos;

    private TrainSpot GetClosestSpot(Vector2 pos)
    {
        var spots = trainSpots.Where(x => Vector2.Distance(x.transform.position, pos) < .35f); // sqrt(2) / 4
        if (spots.Count() == 0)
            return null;

        return spots.OrderBy(x => Vector2.Distance(x.transform.position, pos)).First();
    }

    private IEnumerator AddPeopleOnPlateform()
    {
        while (true)
        {
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

            // Get sprite that is not the same as an adjacent one
            var colorsAvailable = GetAvailableColors(pattern, posOnPlateform.Value);
            int randomColor;
            if (colorsAvailable.Count > 0)
                randomColor = colorsAvailable[Random.Range(0, colorsAvailable.Count)];
            else
                randomColor = Random.Range(0, sprites.Length) + 1;
            Sprite sprite = sprites[randomColor - 1]; // randomColor go from 1 to sprites.Length so we remove one to be in the bounds

            foreach (var pos in pattern)
                plateform[pos.x + posOnPlateform.Value.x, pos.y + posOnPlateform.Value.y] = randomColor;

            GameObject group = new GameObject("Group " + groupNb, typeof(PeopleGroup));
            foreach (Vector2Int pos in pattern)
            {
                GameObject go = Instantiate(peoplePrefab, group.transform);
                go.transform.position = (Vector2)pos + posOnPlateform.Value + new Vector2(plateformPosX, plateformPosY);
                go.GetComponent<SpriteRenderer>().sprite = sprite;
            }

            groupNb++;
            yield return new WaitForSeconds(2f);
        }
    }

    /// <summary>
    /// Make sure we don't generate a group of people with the same sprite as an adjacent one
    /// </summary>
    private List<int> GetAvailableColors(Vector2Int[] pattern, Vector2Int offset)
    {
        List<int> colorsAvailable = new List<int>();
        for (int i = 1; i <= sprites.Length; i++)
            colorsAvailable.Add(i);
        foreach (var pos in pattern)
        {
            RemoveColor(colorsAvailable, offset.x + pos.x - 1, offset.y + pos.y);
            RemoveColor(colorsAvailable, offset.x + pos.x + 1, offset.y + pos.y);
            RemoveColor(colorsAvailable, offset.x + pos.x, offset.y + pos.y - 1);
            RemoveColor(colorsAvailable, offset.x + pos.x, offset.y + pos.y + 1);
        }
        return colorsAvailable;
    }

    private void RemoveColor(List<int> list, int x, int y)
    {
        if (x < 0 || y < 0 || x >= plateformX || y >= plateformY) // Out of bounds
            return;

        int value = plateform[x, y];
        if (value != 0)
            list.Remove(value);
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
