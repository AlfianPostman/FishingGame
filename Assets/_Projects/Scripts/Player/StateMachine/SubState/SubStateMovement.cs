using UnityEngine;

public class SubStateMovement : SuperStateGround 
{
    public SubStateMovement(PlayerGeneral PLAYERGENERAL, PlayerStateMachine PLAYERSTATEMACHINE) : base(PLAYERGENERAL, PLAYERSTATEMACHINE)
    {
        
    }

    public override void Update() 
    {
        base.Update();
        if (PLAYER.INPUTPROCESSOR.INPUTVECTOR.magnitude == 0)
        {
            PLAYERSTATEMACHINE.Change(PLAYER.STATES.IDLESTATE);
        }
    }
    
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        PLAYER.MOVEMENT.VelocityMovement(PLAYER.INPUTPROCESSOR.INPUTVECTORNORMAL);
    }
}