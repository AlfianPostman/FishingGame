using UnityEngine;

public class SuperStateAir : PlayerStateManager 
{
    public SuperStateAir(PlayerGeneral PLAYERGENERAL, PlayerStateMachine PLAYERSTATEMACHINE) : base(PLAYERGENERAL, PLAYERSTATEMACHINE)
    {

    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        PLAYER.MOVEMENT.VelocityMovement(PLAYER.INPUTPROCESSOR.INPUTVECTORNORMAL);
    }
}