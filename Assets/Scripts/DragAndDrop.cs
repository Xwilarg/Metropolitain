using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DragAndDrop : MonoBehaviour
{
    private PeopleGroup group;
    private BoxCollider2D boxCollider;

    private void Start()
    {
        group = transform.parent.GetComponent<PeopleGroup>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void OnMouseDown()
    {
        group.BeginDrag();
        boxCollider.enabled = false;
    }

    private void OnMouseUp()
    {
        group.StopDrag();
        boxCollider.enabled = true;
    }
}
