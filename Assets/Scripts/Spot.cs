using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Spot : MonoBehaviour
{
    private MapManager trainSpot;

    private void OnMouseUp()
    {
        Debug.Log("trainspot"); 
    }



}
