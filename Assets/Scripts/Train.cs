using UnityEngine;

public class Train : MonoBehaviour
{
    [SerializeField]
    private GameObject trainTile;

    private MapManager mm;
    private const float speed = .05f;
    private readonly Vector2 startPos = Vector2.down * 25f;

    // Train position
    private const int trainTileX = -7, trainTileY = -5;

    private void Start()
    {
        mm = GameObject.FindGameObjectWithTag("GameController").GetComponent<MapManager>();
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
    }

    private void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, Vector2.zero, speed);
    }
}
