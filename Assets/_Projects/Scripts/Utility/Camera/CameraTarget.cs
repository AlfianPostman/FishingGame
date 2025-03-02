using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    [SerializeField] float speed;
    float baseSpeed;
    float interactingSpeed;

    [SerializeField] Transform camTarget;
    Transform playerPosition;
    Transform currentTarget;

    private void Start()
    {
        playerPosition = transform;
        currentTarget = playerPosition;

        baseSpeed = speed;
        interactingSpeed = speed/4;
    }

    void FixedUpdate()
    {
        FollowTarget();
    }

    void FollowTarget()
    {
        if (currentTarget != playerPosition)
            speed = interactingSpeed;
        else
            speed = baseSpeed;

        camTarget.transform.position = Vector3.Lerp(camTarget.position, currentTarget.position, speed);
    }

    public void ChangeTargetTo(Transform target)
    {
        currentTarget = target;
    }

    public void ResetTarget()
    {
        currentTarget = playerPosition;
    }
}
