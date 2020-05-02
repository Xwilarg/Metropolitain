using UnityEngine;
using UnityEngine.UI;

public class Train : MonoBehaviour
{
    [SerializeField]
    private GameObject trainTile;

    [SerializeField]
    private Text timerText; 

    private MapManager mm;
    private GameOverManager gm;
    private const float speed = .05f;
    private readonly Vector2 startPos = Vector2.up * 25f;
    private const float timerRef = 20f;
    private float timer;

    private Vector2 obj; // Where the train need to go

    // Train position
    private const int trainTileX = -7, trainTileY = -4;

    private void Start()
    {
        var gc = GameObject.FindGameObjectWithTag("GameController");
        mm = gc.GetComponent<MapManager>();
        gm = gc.GetComponent<GameOverManager>();
        int trainX = mm.GetTrainX();
        int trainY = mm.GetTrainY();
        for (int x = 0; x < trainX; x++)
        {
            for (int y = 0; y < trainY; y++)
            {
                var tile = Instantiate(trainTile, transform);
                tile.transform.position = new Vector2(x + trainTileX, y + trainTileY);
                var spot = tile.GetComponent<TrainSpot>();
                spot.Position = new Vector2Int(x, y);
                mm.SetSpot(x + (y * trainX), spot);
            }
        }
        transform.position = startPos;
        timer = timerRef;
        timerText.text = timer.ToString("0.00").Replace(',', '.');
        obj = Vector2.zero;
    }

    public void AddToTrain(int x, int y)
    {
        GameObject g = new GameObject("Group X", typeof(SpriteRenderer));
        g.transform.parent = transform;
        g.GetComponent<SpriteRenderer>().sprite = mm.GetSprites()[0];
        g.transform.position = transform.position + new Vector3(x, y) + new Vector3(trainTileX, trainTileY);
    }

    private void FixedUpdate()
    {
        if (gm.GameOver)
            return;
        transform.position = Vector3.Lerp(transform.position, obj, speed);
    }

    private void Update()
    {
        if (gm.GameOver)
            return;
        timer -= Time.deltaTime;
        if (timer < 0f)
        {
            NextTrain();
            if (transform.position.y - obj.y < 1f)
            {
                timer = timerRef;
                obj = Vector2.zero;
                transform.position = startPos;
                for (int i = 0; i < transform.childCount; i++)
                {
                    var child = transform.GetChild(i);
                    if (child.name.StartsWith("Group"))
                        Destroy(child.gameObject);
                }
                mm.IncreaseWagonCount();
                mm.CleanTrain();
            }
        }
        else if (transform.position.y - obj.y > 1f)
            timer = timerRef;
        timerText.text = timer.ToString("0.00").Replace(',', '.');
    }

    public void NextTrain()
    {
        timer = 0f;
        obj = -startPos;
    }
}
