using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;

    public float distance;
    public float sensitivity = 5.0f;
    public float fixedVerticalAngle = 45f;

    private float _currentX = 0.0f;

    void FixedUpdate()
    {
        // Mouse drag input (right-click)
        if (Input.GetMouseButton(1))
        {
            _currentX += Input.GetAxis("Mouse X") * sensitivity;
            
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        // Touch swipe input (mobile)
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            Touch touch = Input.GetTouch(0);
            _currentX += touch.deltaPosition.x * sensitivity * Time.deltaTime;
        }
    }

    void LateUpdate()
    {
        // Calculate rotation with fixed vertical angle and current horizontal rotation
        Quaternion rotation = Quaternion.Euler(fixedVerticalAngle, _currentX, 0);
        Vector3 position = target.position + rotation * new Vector3(0, 0, -distance);
        
        // Update camera position and rotation
        transform.position = position;
        transform.LookAt(target);
    }
}
