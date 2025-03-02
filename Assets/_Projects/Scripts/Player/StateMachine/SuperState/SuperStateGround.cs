using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SuperStateGround : PlayerStateManager 
{
    public SuperStateGround(PlayerGeneral PLAYERGENERAL, PlayerStateMachine PLAYERSTATEMACHINE) : base(PLAYERGENERAL, PLAYERSTATEMACHINE)
    {

    }

    public override void Update()
    {
        base.Update();
        if(!PLAYER.COLLISION.GROUND)
        {
            PLAYERSTATEMACHINE.Change(PLAYER.STATES.INAIRSTATE);
        }
        
        if (PLAYER.COLLISION.GROUND && PLAYER.INPUTPROCESSOR.INPUTJUMP > Time.time)
        {
            PLAYERSTATEMACHINE.Change(PLAYER.STATES.JUMPSTATE);
        }

        if (PLAYER.COLLISION.GROUND && PLAYER.INPUTPROCESSOR.INPUTINTERACT > Time.time && PLAYER.COLLISION.INTERACT.Length > 0)
        {
            PLAYERSTATEMACHINE.Change(PLAYER.STATES.INTERACTSTATE);
        }

        if (PLAYER.COLLISION.GROUND && PLAYER.INPUTPROCESSOR.INPUTACTION > Time.time && !PLAYER.FISHINGCONTROLLER.onCooldown)
        {
            PLAYERSTATEMACHINE.Change(PLAYER.STATES.ACTIONSTATE);
        }

        if (Input.GetKey(KeyCode.Escape)) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}