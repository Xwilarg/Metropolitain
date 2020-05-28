using UnityEngine;

public class Pause : MonoBehaviour
{
    public void TogglePause()
    {
        Time.timeScale = Time.timeScale == 1 ? 0 : 1;
    }
}
