using UnityEngine;

public class PeopleGroup : MonoBehaviour
{
    private bool isDrag;
    private Vector2 mouseOffset;

    private void Start()
    {
        isDrag = false;
    }

    private void Update()
    {
        if (isDrag)
            transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - mouseOffset;
    }

    public void BeginDrag()
    {
        isDrag = true;
        mouseOffset = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - (Vector2)transform.position;
    }

    public void StopDrag()
        => isDrag = false;
}
