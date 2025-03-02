using UnityEngine;

public class NPCCollision : MonoBehaviour
{
    NPCGeneral npc;

    void Awake()
    {
        npc = GetComponentInParent<NPCGeneral>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            npc.showBubbleButton();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            npc.hideBubbleButton();
        }
    }
}
