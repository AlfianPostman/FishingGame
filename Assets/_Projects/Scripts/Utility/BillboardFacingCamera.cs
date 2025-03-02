using UnityEngine;

public class BillboardFacingCamera : MonoBehaviour 
{
    private Transform cam;

    private void Start() 
    {
        cam = Camera.main.transform;
    }

    private void Update() 
    {
        transform.LookAt(cam);
        transform.RotateAround(transform.position, transform.up, 180f);
    }
}