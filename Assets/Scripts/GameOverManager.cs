using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    [SerializeField]
    private GameObject gameOverText;

    public bool GameOver { private set; get; }

    private void Start()
    {
        GameOver = false;
    }

    public void Loose()
    {
        GameOver = true;
        gameOverText.SetActive(true);
    }
}
