using System;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class InputProcessor 
{
    public InputProcessor()
    {

    }

    Vector2 inputVector;
    public Vector2 INPUTVECTOR => inputVector;
    public Vector2 INPUTVECTORNORMAL => inputVector.normalized;

    float inputJump;
    public float INPUTJUMP => inputJump;

    float inputInteract;
    public float INPUTINTERACT => inputInteract;
    
    float InputAction;
    public float INPUTACTION => InputAction;

    public void ProcessInputVector(Vector2 value)
    {
        this.inputVector = value;
    }

    public void ProcessInputJump(float value)
    {
        this.inputJump = value;
    }

    public void ProcessInputInteract(float value)
    {
        this.inputInteract = value;
    }
    
    public void ProcessInputAction(float value)
    {
        this.InputAction = value;
    }
}