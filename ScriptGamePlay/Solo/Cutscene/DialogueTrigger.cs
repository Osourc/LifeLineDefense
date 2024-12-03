using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue[] introDialogue;
    public Dialogue[] endingDialogue;

    private Dictionary<string, Dialogue[]> dialogues;

    private void Awake()
    {
        dialogues = new Dictionary<string, Dialogue[]>
        {
            { "intro", introDialogue},
            { "ending", endingDialogue}
        };
    }

    public void TriggerDialogue(string type, int DialogueIndex) 
    {
        if (dialogues.ContainsKey(type) && DialogueIndex < dialogues[type].Length)
        {
            FindAnyObjectByType<CutsceneManagerScript>().StartDialogue(dialogues[type][DialogueIndex]);
        }
        else
        {
            Debug.LogWarning($"Dialogue type '{type}' not found or index out of range!");
        }
    }
}
