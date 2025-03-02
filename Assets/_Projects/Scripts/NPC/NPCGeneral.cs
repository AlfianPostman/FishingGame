using TMPro;
using UnityEngine;

public class NPCGeneral : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] Rigidbody rb;
    [SerializeField] TextMeshPro text;

    [Header("Dialogue")]
    public DialogueLine[] dialogue;
    public DialogueManager dialogueManager;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();    
    }

    public void OnInteract()
    {
        rb.linearVelocity += Vector3.up * 5f;
        hideBubbleButton();
        dialogueManager.StartDialogue(dialogue, transform.position);
    }

    public void showBubbleButton()
    {
        text.enabled = true;
    }

    public void hideBubbleButton()
    {
        text.enabled = false;
    }
}