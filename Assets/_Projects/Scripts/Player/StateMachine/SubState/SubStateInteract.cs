using UnityEngine;
using System.Linq;

public class SubStateInteract : SuperStateAction
{
    float enterTime;
    float nextLineTime;
    Transform closestObj;
    Collider closestCollider;

    public SubStateInteract(PlayerGeneral PLAYERGENERAL, PlayerStateMachine PLAYERSTATEMACHINE) : base(PLAYERGENERAL, PLAYERSTATEMACHINE)
    {
    }

    public override void Enter()
    {
        base.Enter();
        PLAYER.MOVEMENT.VelocityIdle();

        closestCollider = PLAYER.COLLISION.INTERACT.OrderBy(c => Vector3.Distance(PLAYER.transform.position, c.transform.position)).FirstOrDefault();
        closestObj = closestCollider.transform;
        closestCollider.GetComponentInParent<IInteractable>().OnInteract();

        PLAYER.CAMERATARGET.ChangeTargetTo(closestObj);

        enterTime = Time.time;
        nextLineTime = Time.time;
    }

    public override void Update()
    {
        base.Update();

        PLAYER.MOVEMENT.PlayerLookAt(closestObj);

        if (PLAYER.INPUTPROCESSOR.INPUTINTERACT > Time.time && Time.time > nextLineTime + 1f)
        {
            nextLineTime = Time.time;

            PLAYER.DIALOGUEMANAGER.NextLine();
        }

        if (PLAYER.DIALOGUEMANAGER.isEndOfLine && Time.time > nextLineTime + .5f)
        {
            PLAYERSTATEMACHINE.Change(PLAYER.STATES.IDLESTATE);
        }

    }

    public override void Exit()
    {
        base.Exit();
        closestCollider.GetComponentInParent<NPCGeneral>().showBubbleButton();
        PLAYER.CAMERATARGET.ResetTarget();
    }
}