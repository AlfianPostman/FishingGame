using UnityEngine;

public class SubStateIdle : SuperStateGround 
{
    public SubStateIdle(PlayerGeneral PLAYERGENERAL, PlayerStateMachine PLAYERSTATEMACHINE) : base(PLAYERGENERAL, PLAYERSTATEMACHINE)
    {

    }
    
    public override void Enter()
    {
        base.Enter();
        PLAYER.MOVEMENT.VelocityIdle();
    }

    public override void Update()
    {
        base.Update();
        if (PLAYER.INPUTPROCESSOR.INPUTVECTOR.magnitude > 0)
        {
            PLAYERSTATEMACHINE.Change(PLAYER.STATES.MOVESTATE);
        }
    }
}