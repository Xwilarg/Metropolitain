using UnityEngine;

public class PeopleGroup : MonoBehaviour
{
    private bool isDrag;
    private bool isLocked;
    private Vector2 mouseOffset;
    private Vector3 initPos;
    private float speed = .3f;
    private Transform[] children;
    private Transform trainTransform;

    private Vector2Int[] dest; // Keep track of the differents pos of the block for MapManager
    public Vector2Int[] GetDest() => dest;

    private MapManager mm;
    private GameOverManager gm;

    private void Start()
    {
        isDrag = false;
        isLocked = false;
        var gc = GameObject.FindGameObjectWithTag("GameController");
        mm = gc.GetComponent<MapManager>();
        gm = gc.GetComponent<GameOverManager>();

        // Store all children transform
        children = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            children[i] = transform.GetChild(i).transform;

        trainTransform = GameObject.FindGameObjectWithTag("Train").transform;
    }

    public void SetDestination(Vector3 value, Vector2Int[] finalPos)
    {
        initPos = value;
        dest = finalPos;
    }

    public Vector3 GetInitPos() => initPos;

    private void FixedUpdate()
    {
        if (!isDrag && !gm.GameOver)
        {
            if (isLocked && trainTransform.position != Vector3.zero) // Train is moving and object is attached to it
            {
                transform.position = initPos + trainTransform.position;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, initPos, speed);
            }
        }
    }

    private void Update()
    {
        if (isDrag && !gm.GameOver)
            transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - mouseOffset;
    }

    public void BeginDrag()
    {
        if (gm.GameOver)
            return;
        isDrag = true;
        mouseOffset = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - (Vector2)transform.position;
    }

    public void StopDrag()
    {
        if (gm.GameOver)
            return;
        isDrag = false;

        // Check if all child is on a valid position in the train
        foreach (Transform t in children)
        {
            if (!mm.IsPositionOnTrain(t.position)) // Invalid position
                return;
        }

        // Say to MapManager where all people are
        foreach (Transform t in children)
            mm.LockPositionOnTrain(t.position);
        foreach (Vector2Int v in dest)
            mm.UnlockPositionOnPlateform(v, this);
        transform.parent = trainTransform;
        isLocked = true;

        // Remove collider so we can't move them anymore
        foreach (BoxCollider2D coll in GetComponentsInChildren<BoxCollider2D>())
            coll.enabled = false;

        initPos = (Vector2)transform.position + mm.GetOffset(children[0].position);

        mm.UpdatePlateform();
    }
}
