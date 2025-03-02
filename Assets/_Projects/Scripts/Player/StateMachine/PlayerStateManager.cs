using UnityEngine;

public class PlayerStateManager
{
    protected PlayerStateMachine PLAYERSTATEMACHINE;
    protected PlayerGeneral PLAYER;

    public PlayerStateManager(PlayerGeneral PLAYERGENERAL, PlayerStateMachine PLAYERSTATEMACHINE)
    {
        this.PLAYERSTATEMACHINE = PLAYERSTATEMACHINE;
        this.PLAYER = PLAYERGENERAL;
    }

    public virtual void Enter() { }

    public virtual void Exit() { }

    public virtual void Update() { }

    public virtual void FixedUpdate() { }

    public virtual void LateUpdate() { }
}
