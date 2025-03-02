using UnityEngine;

public class PlayerMovement 
{
    Transform player;
    Transform cam;
    Rigidbody rb;
    PlayerConfiguration PlayerConfiguration;

    public PlayerMovement(Rigidbody rb, PlayerConfiguration playerConfiguration, Transform player)
    {
        this.rb = rb;
        this.PlayerConfiguration = playerConfiguration;
        this.player = player;
        cam = Camera.main.transform;
    }
    
    public void VelocityMovement(Vector2 inputVec)
    {
        // Normalize the camera angle
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward = camForward.normalized;
        camRight = camRight.normalized;

        // Start moving
        Vector3 yVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        rb.linearVelocity = inputVec.x * camRight * PlayerConfiguration.MOVESPEED + inputVec.y * camForward * PlayerConfiguration.MOVESPEED + yVelocity;

        PlayerRotation();
    }

    public void VelocityIdle()
    {
        rb.linearVelocity = Vector3.zero;
    }

    public void PlayerRotation()
    {
        Vector3 velocityWithoutY = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).normalized;
        if (velocityWithoutY != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(velocityWithoutY.normalized, Vector3.up);
            Quaternion newRotation = Quaternion.Euler(0f, Quaternion.Lerp(rb.rotation, targetRotation, PlayerConfiguration.TURNSPEED * Time.deltaTime).eulerAngles.y, 0f);
            rb.MoveRotation(newRotation);
        }
    }

    public void PlayerLookAt(Transform target)
    {
        Vector3 direction = target.position - player.position;
        direction.y = 0f;
        Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
        Quaternion SmoothedRotation = Quaternion.Euler(0f, Quaternion.Lerp(rb.rotation, lookRotation, PlayerConfiguration.TURNSPEED * Time.deltaTime).eulerAngles.y, 0f);

        rb.MoveRotation(SmoothedRotation);
    }

    public void VelocityJump()
    {
        rb.linearVelocity = Vector3.up * PlayerConfiguration.JUMPFORCE;
    }

    public float VELOCITYY => rb.linearVelocity.y;
}