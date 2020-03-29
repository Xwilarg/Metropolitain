﻿using UnityEngine;
using UnityEngine.UI;

public class Train : MonoBehaviour
{
    [SerializeField]
    private GameObject trainTile;

    [SerializeField]
    private Text timerText; 

    private MapManager mm;
    private const float speed = .05f;
    private readonly Vector2 startPos = Vector2.down * 25f;
    private const float timerRef = 10f;
    private float timer;

    private Vector2 obj; // Where the train need to go

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
        timer = timerRef;
        timerText.text = timer.ToString("0.00").Replace(',', '.');
        obj = Vector2.zero;
    }

    private void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, obj, speed);
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0f)
        {
            timer = 0f;
            obj = -startPos;
            if (obj.y - transform.position.y < 1f)
            {
                timer = timerRef;
                obj = Vector2.zero;
                transform.position = startPos;
                // TODO: Clean train
            }
        }
        timerText.text = timer.ToString("0.00").Replace(',', '.');
    }
}