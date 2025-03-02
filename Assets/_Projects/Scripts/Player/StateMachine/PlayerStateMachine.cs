using UnityEngine;

public class PlayerStateMachine 
{
    public PlayerStateManager state {get; private set;}

    public void Init(PlayerStateManager state)
    {
        this.state = state;
        this.state.Enter();
    }

    public void Change(PlayerStateManager state)
    {
        this.state.Exit();
        Init(state);
    }
}