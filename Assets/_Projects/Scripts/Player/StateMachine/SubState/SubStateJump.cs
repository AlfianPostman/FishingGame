using UnityEngine;

public class SubStateJump : SuperStateAir
{
    public SubStateJump(PlayerGeneral PLAYERGENERAL, PlayerStateMachine PLAYERSTATEMACHINE) : base(PLAYERGENERAL, PLAYERSTATEMACHINE)
    {

    }

    public override void Enter()
    {
        base.Enter();
        PLAYER.MOVEMENT.VelocityJump();
        Physics.gravity = Vector3.up * -80f;
    }

    public override void Update()
    {
        base.Update();
        if(PLAYER.MOVEMENT.VELOCITYY < 0)
        {
            PLAYERSTATEMACHINE.Change(PLAYER.STATES.INAIRSTATE);
        }
    }

    public override void Exit()
    {
        base.Exit();
        Physics.gravity = Vector3.up * -9.81f;
    }
}