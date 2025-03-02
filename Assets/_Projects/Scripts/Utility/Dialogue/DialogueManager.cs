using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public TextMeshPro dialogueText; // Assign the 3D TextMeshPro object in the Inspector
    private DialogueLine[] dialogueLines;
    private int currentLineIndex = 0;

    public bool isEndOfLine = false;

    public void StartDialogue(DialogueLine[] lines, Vector3 npcPosition)
    {
        dialogueLines = lines;
        currentLineIndex = 0;
        isEndOfLine = false;

        // Position the text above the NPC
        dialogueText.transform.position = npcPosition + Vector3.up * 3f; // Adjust height as needed
        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (currentLineIndex < dialogueLines.Length)
        {
            dialogueText.text = dialogueLines[currentLineIndex].text;
        }
        else
        {
            isEndOfLine = true;
            EndDialogue();
        }
    }

    public void NextLine()
    {
        currentLineIndex++;

        ShowCurrentLine();
    }

    private void EndDialogue()
    {
        dialogueText.text = ""; // Clear the text when dialogue ends
    }
}