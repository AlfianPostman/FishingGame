using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] PlayerGeneral PLAYER;

    public void InputMovement(InputAction.CallbackContext context)
    {
        PLAYER.INPUTPROCESSOR.ProcessInputVector(context.ReadValue<Vector2>());
    }

    public void InputJump(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            float time = (float)context.time;
            PLAYER.INPUTPROCESSOR.ProcessInputJump(time);
        }
        else
        {
            PLAYER.INPUTPROCESSOR.ProcessInputJump(0);
        }
    }
    
    public void InputInteract(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            float time = (float)context.time;
            PLAYER.INPUTPROCESSOR.ProcessInputInteract(time);
        }
        else
        {
            PLAYER.INPUTPROCESSOR.ProcessInputInteract(0);
        }
    }
    
    public void InputAction(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            float time = (float)context.time;
            PLAYER.INPUTPROCESSOR.ProcessInputAction(time);
        }
        else
        {
            PLAYER.INPUTPROCESSOR.ProcessInputAction(0);
        }
    }
}