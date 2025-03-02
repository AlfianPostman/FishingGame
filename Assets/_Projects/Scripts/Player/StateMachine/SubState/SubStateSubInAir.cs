using UnityEngine;

public class SubStateInAir : SuperStateAir
{
    public SubStateInAir(PlayerGeneral PLAYERGENERAL, PlayerStateMachine PLAYERSTATEMACHINE) : base(PLAYERGENERAL, PLAYERSTATEMACHINE)
    {

    }

    public override void Enter()
    {
        Physics.gravity = Vector3.up * -80f;
    }

    public override void Update()
    {
        base.Update();
        if(PLAYER.COLLISION.GROUND)
        {
            if(PLAYER.INPUTPROCESSOR.INPUTVECTORNORMAL.magnitude == 0)
            {
                PLAYERSTATEMACHINE.Change(PLAYER.STATES.IDLESTATE);
            }
            else
            {
                PLAYERSTATEMACHINE.Change(PLAYER.STATES.MOVESTATE);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        Physics.gravity = Vector3.up * -9.81f;
    }
}