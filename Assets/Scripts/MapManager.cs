using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("All character's sprites")]
    private Sprite[] sprites;

    [SerializeField]
    private GameObject peoplePrefab;

    [SerializeField]
    private Text scoreText, highscoreText, wagonText;

    [SerializeField]
    private GameObject borderUp, borderLeft, borderDown, borderRight;

    private List<(int, Vector2Int[])> patterns;

    // Keep track of the people on the plateform and in the train
    private int[,] plateform;
    private bool[,] train;

    // To keep track of the number of group that were spawned, mostly for debug purpose
    private int groupNb;

    // Dimensions for plateform and train
    private const int plateformX = 3, plateformY = 8;
    private const int trainX = 5, trainY = 8;
    public int GetTrainX() => trainX;
    public int GetTrainY() => trainY;

    // Plateform position
    private const float plateformPosX = 0f, plateformPosY = -4f;

    private TrainSpot[] trainSpots = new TrainSpot[trainX * trainY];

    private GameOverManager gm;

    private List<PeopleGroup> groups; // Keep track of groups of people

    private int score;
    private int highscore;
    private int baseHighscore;

    private int wagonCount;

    private void Start()
    {
        score = 0;
        if (PlayerPrefs.HasKey("highscore"))
            highscore = PlayerPrefs.GetInt("highscore");
        else
            highscore = 0;
        baseHighscore = highscore;
        wagonCount = 0;

        // Ids are used for falling orders in pieces.txt
        patterns = new List<(int, Vector2Int[])>
        {
            // 1x1
            (1, new[] { Vector2Int.zero }), // ID: 1

            // L Shape
            (2, new[] { Vector2Int.up, Vector2Int.one, Vector2Int.right * 2, new Vector2Int(2, 1) }), // ID: 2
            (2, new[] { Vector2Int.zero, Vector2Int.right, Vector2Int.one, new Vector2Int(1, 2) }), // ID: 3
            (2, new[] { Vector2Int.zero, Vector2Int.up, Vector2Int.right, Vector2Int.right * 2 }), // ID: 4
            (2, new[] { Vector2Int.zero, Vector2Int.up, Vector2Int.up * 2, new Vector2Int(1, 2) }), // ID: 5
            (2, new[] { Vector2Int.zero, Vector2Int.right, Vector2Int.right * 2, new Vector2Int(2, 1) }), // ID: 6
            (2, new[] { Vector2Int.zero, Vector2Int.up, Vector2Int.up * 2, Vector2Int.right }), // ID: 7
            (2, new[] { Vector2Int.zero, Vector2Int.up, Vector2Int.one, new Vector2Int(2, 1) }), // ID: 8
            (2, new[] { Vector2Int.right, Vector2Int.one, Vector2Int.up * 2, new Vector2Int(1, 2) }), // ID: 9

            // 2x1
            (3, new[] { Vector2Int.zero, Vector2Int.right }), // ID: 10
            (3, new[] { Vector2Int.zero, Vector2Int.up }), // ID: 11

            // 2x2
            (4, new[] { Vector2Int.zero, Vector2Int.right, Vector2Int.up, Vector2Int.one }), // ID: 11

            // S Shape
            (5, new[] { Vector2Int.up, Vector2Int.one, Vector2Int.right, Vector2Int.right * 2 }), // ID: 12
            (5, new[] { Vector2Int.zero, Vector2Int.up, Vector2Int.one, new Vector2Int(1, 2) }), // ID: 13
            (5, new[] { Vector2Int.zero, Vector2Int.right, Vector2Int.one, new Vector2Int(2, 1) }), // ID: 14
            (5, new[] { Vector2Int.up, Vector2Int.one, Vector2Int.right, Vector2Int.up * 2 }) // ID: 15
        };

        plateform = new int[plateformX, plateformY];
        train = new bool[trainX, trainY];
        groups = new List<PeopleGroup>();
        for (var i = 0; i < plateformX * plateformY; i++)
            plateform[i % plateformX, i / plateformY] = 0;
        for (int x = 0; x < trainX; x++)
            for (int y = 0; y < trainY; y++)
                train[x, y] = true;

        Transform borders = new GameObject("Borders").transform;
        // Draw plateform
        for (int x = 0; x < plateformX; x++)
        {
            Instantiate(borderUp, new Vector2(plateformPosX + x, plateformPosY - 1), Quaternion.identity).transform.parent = borders;
            Instantiate(borderDown, new Vector2(plateformPosX + x, plateformPosY + plateformY + 1), Quaternion.identity).transform.parent = borders;
        }
        for (int y = 0; y <= plateformY; y++)
        {
            Instantiate(borderRight, new Vector2(plateformPosX - 1, plateformPosY + y), Quaternion.identity).transform.parent = borders;
            Instantiate(borderLeft, new Vector2(plateformPosX + plateformX, plateformPosY + y), Quaternion.identity).transform.parent = borders;
        }

        groupNb = 0;

        gm = GetComponent<GameOverManager>();

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

    public void UnlockPositionOnPlateform(Vector2Int pos, PeopleGroup pg)
    {
        plateform[pos.x, pos.y] = 0;
        groups.Remove(pg);
    }

    public void CleanTrain()
    {
        for (int x = 0; x < trainX; x++)
            for (int y = 0; y < trainY; y++)
            {
                score += train[x, y] ? 0 : 1;
            }
        if (score > highscore)
        {
            if (baseHighscore > highscore)
                highscore = baseHighscore;
            else
                highscore = score;
            highscoreText.text = "High Score: " + highscore;
        }
        scoreText.text = "Score: " + score;

        for (int x = 0; x < trainX; x++)
            for (int y = 0; y < trainY; y++)
                train[x, y] = true;
    }

    public void UpdatePlateform()
    {
    checkGroup:
        foreach (var group in groups) // We check for each group if we can move it
        {
            var pos = group.GetDest();
            var lowestY = pos.Min(x => x.y); // We get the lowest position of the block
            var minX = pos.Min(x => x.x);
            var maxX = pos.Max(x => x.x);
            bool isOkay = true;
            for (int i = minX; i <= maxX; i++) // Check if we can move the piece of one to the bottom
            {
                if (!IsPlateformPosFree(i, lowestY - 1))
                {
                    isOkay = false;
                    break;
                }
            }
            if (!isOkay)
                continue;
            foreach (var d in pos)
                plateform[d.x, d.y] = 0;
            foreach (var d in pos)
                plateform[d.x, d.y - 1] = 1;
            for (int i = 0; i < pos.Length; i++)
                pos[i] = new Vector2Int(pos[i].x, pos[i].y - 1);
            group.SetDestination(group.GetInitPos() + Vector3.down, pos);
            goto checkGroup; // If we can, we redo the check with all pieces
        }
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
        int maxPattern = patterns.Max(x => x.Item1);
        while (!gm.GameOver)
        {
            var randomPatternType = Random.Range(0, maxPattern) + 1;
            var patternTypeList = patterns.Where(y => y.Item1 == randomPatternType);
            var pattern = patternTypeList.ElementAt(Random.Range(0, patternTypeList.Count())).Item2;
            int xMax = plateformX - GetXPatternLength(pattern); // Check what is the max x pos depending of the length of the selected pattern
            int xPos = Random.Range(0, xMax);

            int color = Random.Range(0, sprites.Length);
            Sprite sprite = sprites[color]; // randomColor go from 1 to sprites.Length so we remove one to be in the bounds

            int yPos = DoesPatternFitOnPlateform(xPos, pattern);
            if (yPos == -1)
            {
                PlayerPrefs.SetInt("highscore", highscore);
                gm.Loose();
            }
            else
            {
                Vector2Int[] finalPos = new Vector2Int[pattern.Length];
                int i = 0;
                foreach (var pos in pattern)
                {
                    finalPos[i] = new Vector2Int(pos.x + xPos, pos.y + yPos);
                    plateform[pos.x + xPos, pos.y + yPos] = 1;
                    i++;
                }

                GameObject group = new GameObject("Group " + groupNb, typeof(PeopleGroup));
                Vector3? dest = null;
                foreach (Vector2Int pos in pattern)
                {
                    GameObject go = Instantiate(peoplePrefab, group.transform);
                    var tmp = pos + new Vector2(xPos, yPos) + new Vector2(plateformPosX, plateformPosY);
                    if (dest == null) dest = tmp;
                    go.transform.position = new Vector2(tmp.x, -plateformPosY + pos.y);
                    go.GetComponent<SpriteRenderer>().sprite = sprite;
                }
                var g = group.GetComponent<PeopleGroup>();
                g.SetDestination(new Vector2(0f, dest.Value.y + plateformPosY), finalPos);
                groups.Add(g);

                groupNb++;
            }
            yield return new WaitForSeconds(3f);
        }
    }

    /// <summary>
    /// Check if a position on the plateform is occupied or not
    /// <returns></returns>
    private bool IsPlateformPosFree(int x, int y)
        => x >= 0 && y >= 0 && x < plateformX && y < plateformY && plateform[x, y] == 0;

    /// <summary>
    /// Check if we can put a pattern on the y position of a plateform
    /// </summary>
    /// <returns>Return X pos of the pattern or -1 if it doesn't fit</returns>
    private int DoesPatternFitOnPlateform(int x, Vector2Int[] pattern)
    {
        int y = plateformY - 1;
        for (; y >= 0; y--)
        {
            if (!IsPlateformPosFree(x, y))
                break;
        }
        y++;
        for (; y < plateformY - 1; y++) // We find the first free space available on the line Y
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

    public void IncreaseWagonCount()
    {
        wagonCount++;
        wagonText.text = "Wagon count: " + wagonCount;
    }
}
