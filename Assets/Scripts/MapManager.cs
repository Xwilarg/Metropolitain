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

    private int GetXPatternLength(Vector2Int[] pattern)
    {
        return pattern.OrderByDescending(x => x.x).First().x;
    }

    private IEnumerator AddPeopleOnPlateform()
    {
        while (true)
        {
            List<Vector2Int[]> allPatterns = new List<Vector2Int[]>(patterns);
            var pattern = patterns[Random.Range(0, patterns.Count)];
            int xMax = plateformX - GetXPatternLength(pattern); // Check what is the max x pos depending of the length of the selected pattern
            int xPos = Random.Range(0, xMax);

            int color = Random.Range(0, sprites.Length);
            Sprite sprite = sprites[color]; // randomColor go from 1 to sprites.Length so we remove one to be in the bounds

            int yPos = DoesPatternFitOnPlateform(xPos, pattern);
            if (yPos == -1)
            {
                // TODO: GameOver
            }

            foreach (var pos in pattern)
                plateform[pos.x + xPos, pos.y + yPos] = 1;

            GameObject group = new GameObject("Group " + groupNb, typeof(PeopleGroup));
            foreach (Vector2Int pos in pattern)
            {
                GameObject go = Instantiate(peoplePrefab, group.transform);
                go.transform.position = pos + new Vector2(xPos, yPos) + new Vector2(plateformPosX, plateformPosY);
                go.GetComponent<SpriteRenderer>().sprite = sprite;
            }

            groupNb++;
            yield return new WaitForSeconds(2f);
        }
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
    private int DoesPatternFitOnPlateform(int x, Vector2Int[] pattern)
    {
        for (int y = 0; y < plateformY - 1; y++) // We find the first free space available on the line Y
        {
            bool isOkay = true;
            foreach (var elem in pattern)
            {
                if (!IsPlateformPosFree(x + elem.x, y + elem.y))
                {
                    isOkay = false;
                    break;
                }
            }
            if (isOkay)
                return y;
        }
        return -1;
    }
}
