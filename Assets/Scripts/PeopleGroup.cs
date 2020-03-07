using UnityEngine;

public class PeopleGroup : MonoBehaviour
{
    private bool isDrag;
    private Vector2 mouseOffset;
    private Vector3 initPos;
    private float speed = .3f;
    private Transform[] children;

    private MapManager mm;

    private void Start()
    {
        isDrag = false;
        initPos = transform.position;
        mm = GameObject.FindGameObjectWithTag("GameController").GetComponent<MapManager>();

        // Store all children transform
        children = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            children[i] = transform.GetChild(i).transform;
    }

    private void FixedUpdate()
    {
        if (!isDrag)
            transform.position = Vector3.Lerp(transform.position, initPos, speed);
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
    {
        isDrag = false;

        // Check if all child is on a valid position in the train
        foreach (Transform t in children)
        {
            if (!mm.IsPositionOnTrain(t.position)) // Invalid position
                return;
        }

        foreach (Transform t in children)
            mm.LockPositionOnTrain(t.position);

        initPos = (Vector2)transform.position + mm.GetOffset(children[0].position); // TODO: Lock
    }
}
