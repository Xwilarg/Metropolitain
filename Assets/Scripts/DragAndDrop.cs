using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    private PeopleGroup group;

    private void Start()
    {
        group = transform.parent.GetComponent<PeopleGroup>();
    }

    private void OnMouseDown()
    {
        group.BeginDrag();
    }

    private void OnMouseUp()
    {
        group.StopDrag();
    }
}
