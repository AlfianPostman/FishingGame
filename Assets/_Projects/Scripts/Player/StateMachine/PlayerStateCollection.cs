using UnityEngine;

public class PlayerStateCollection 
{
    public SubStateIdle IDLESTATE {get; private set;}
    public SubStateMovement MOVESTATE {get; private set;}
    public SubStateInAir INAIRSTATE {get; private set;}
    public SubStateJump JUMPSTATE {get; private set;}
    public SubStateInteract INTERACTSTATE { get; internal set;}
    public SubStateAction ACTIONSTATE { get; internal set;}
    public SubStateFishing FISHINGSTATE { get; internal set;}

    public PlayerStateCollection(PlayerGeneral playerGeneral, PlayerStateMachine playerStateMachine)
    {
        IDLESTATE = new SubStateIdle(playerGeneral, playerStateMachine);
        MOVESTATE = new SubStateMovement(playerGeneral, playerStateMachine);
        INAIRSTATE = new SubStateInAir(playerGeneral, playerStateMachine);
        JUMPSTATE = new SubStateJump(playerGeneral, playerStateMachine);
        INTERACTSTATE = new SubStateInteract(playerGeneral,playerStateMachine);
        ACTIONSTATE = new SubStateAction(playerGeneral,playerStateMachine);
        FISHINGSTATE = new SubStateFishing(playerGeneral,playerStateMachine);
    }
}