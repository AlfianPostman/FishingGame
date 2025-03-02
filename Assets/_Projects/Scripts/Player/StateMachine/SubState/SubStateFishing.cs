using UnityEngine;

public class SubStateFishing : SuperStateAction
{
    public SubStateFishing(PlayerGeneral PLAYERGENERAL, PlayerStateMachine PLAYERSTATEMACHINE) : base(PLAYERGENERAL, PLAYERSTATEMACHINE)
    {
    }

    float enterTime;

    public override void Enter()
    {
        base.Enter(); 
        enterTime = Time.time;
    }

    public override void Update()
    {
        base.Update();

        // Cancel state region

        if (PLAYER.INPUTPROCESSOR.INPUTACTION > Time.time && Time.time > enterTime + 1f)
        {
            PLAYERSTATEMACHINE.Change(PLAYER.STATES.IDLESTATE);
        }

        if (PLAYER.INPUTPROCESSOR.INPUTVECTOR.magnitude > 0)
        {
            PLAYERSTATEMACHINE.Change(PLAYER.STATES.MOVESTATE);
        }

        if (PLAYER.COLLISION.GROUND && PLAYER.INPUTPROCESSOR.INPUTJUMP > Time.time)
        {
            PLAYERSTATEMACHINE.Change(PLAYER.STATES.JUMPSTATE);
        }

        // do if statement if fish got caught, and start minigame
    }

    public override void Exit()
    {
        base.Exit();

        PLAYER.FISHINGCONTROLLER.bait.ResetBait();
        PLAYER.FISHINGCONTROLLER.StartCooldown();
    }
}