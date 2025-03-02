using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class SubStateAction : SuperStateAction
{
    public SubStateAction(PlayerGeneral PLAYERGENERAL, PlayerStateMachine PLAYERSTATEMACHINE) : base(PLAYERGENERAL, PLAYERSTATEMACHINE)
    {
    }

    bool isCharging;
    bool isCasting;
    bool casted = false;
    float currentPower = 2f;

    public override void Enter()
    {
        base.Enter();
        PLAYER.MOVEMENT.VelocityIdle();
        PLAYER.FISHINGCONTROLLER.anim.SetBool("isCasting", true);

        isCharging = false;
        isCasting = false;
        casted = false;
        Debug.Log("aa");
    }

    public override void Update()
    {
        base.Update();

        if (PLAYER.INPUTPROCESSOR.INPUTACTION > Time.time && !casted)
        {
            Debug.Log("asdasd: " + currentPower);
            isCharging = true;
            currentPower = PLAYER.FISHINGCONTROLLER.minPower;

            if (isCharging)
            {
                currentPower += PLAYER.FISHINGCONTROLLER.chargeSpeed * Time.deltaTime;
                currentPower = Mathf.Clamp(currentPower, PLAYER.FISHINGCONTROLLER.minPower, PLAYER.FISHINGCONTROLLER.maxPower);
            }
        }

        if (PLAYER.INPUTPROCESSOR.INPUTACTION == 0 && !isCasting) // Release and cast
        {
            isCharging = false;
            isCasting = true;
            casted = true;

            Debug.Log("Casting Power: " + currentPower);
            PLAYER.FISHINGCONTROLLER.bait.StartCasting(currentPower);
            PLAYER.FISHINGCONTROLLER.anim.SetBool("isCasting", false);

            //PLAYERSTATEMACHINE.Change(PLAYER.STATES.FISHINGSTATE);
        }

        if (PLAYER.FISHINGCONTROLLER.bait.hasLanded)
        {
            PLAYERSTATEMACHINE.Change(PLAYER.STATES.FISHINGSTATE);
        }

    }

    public override void Exit()
    {
        base.Exit();
    }
}