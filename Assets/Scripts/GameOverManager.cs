using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    public bool GameOver { private set; get; }

    private void Start()
    {
        GameOver = false;
    }

    public void Loose()
    {
        GameOver = true;
    }
}
