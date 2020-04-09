using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [SerializeField]
    private GameObject gameOverUi;

    public bool GameOver { private set; get; }

    private void Start()
    {
        GameOver = false;
    }

    public void Loose()
    {
        GameOver = true;
        gameOverUi.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("Main");
    }
}
