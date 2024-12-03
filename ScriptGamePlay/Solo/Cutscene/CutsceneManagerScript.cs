using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class CutsceneManagerScript : MonoBehaviour
{

    public DialogueTrigger x;
    Animator animator;

    private Queue<string> sentences;
    private Queue<string> nameX;
    private Queue<string> typeX;
    private Queue<Sprite> thumbnail;
    public Image Char1IMG, Char2IMG;
    public GameObject Char1obj, Char2obj;
    int DialogueIndex,DialogueLength;
    public SoloUIMasterScript UIMasterScript;


    // Start is called before the first frame update
    void Start()
    {
        if (UIMasterScript == null)
        {
            UIMasterScript = FindObjectOfType<SoloUIMasterScript>();
        }
        animator = this.GetComponent<Animator>();
        DialogueIndex = 0;
        sentences = new Queue<string>();
        nameX = new Queue<string>();
        typeX = new Queue<string>();
        thumbnail = new Queue<Sprite>();

        x.TriggerDialogue("intro", 0);
        DialogueLength = x.introDialogue.Length - 1;
    }

    /*private void Update()
    {
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            DisplayNextSentence();
        }
    }*/

    public void StartDialogue(Dialogue dialogue) 
    {
        Debug.Log("Starting Dialogue, DialogueIndex: " + DialogueIndex);
        sentences.Clear();
        nameX.Clear();
        typeX.Clear();
        thumbnail.Clear();

        foreach (string sentence in dialogue.sentences) 
        {
            sentences.Enqueue(sentence);
            nameX.Enqueue(dialogue.name);
            typeX.Enqueue(dialogue.type);
            thumbnail.Enqueue(dialogue.thumbnail);
        }
        

        DisplayNextSentence();
    }

    public TMP_Text Char1Name,Char1Dialogue;
    public TMP_Text Char2Name, Char2Dialogue;

    public void DisplayNextSentence() 
    {
        Debug.Log("Displaying Next Sentence, DialogueIndex: " + DialogueIndex);
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        string name = nameX.Dequeue();
        string type = typeX.Dequeue();
        Sprite thumbnailX = thumbnail.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
        if (type == "Char1")
        {
            Char1Name.text = name;
            Char1IMG.sprite = thumbnailX;
            Char2obj.SetActive(false);
            Char1obj.SetActive(true);
        }
        else 
        {
            Char2Name.text = name;
            Char2IMG.sprite = thumbnailX;
            Char2obj.SetActive(true);
            Char1obj.SetActive(false);
        }

    }

    public void prevConvo() 
    {
        Debug.Log("Before prevConvo, DialogueIndex: " + DialogueIndex);
        if (DialogueIndex > 0)
        {
            DialogueIndex--;
        }
        else 
        {
            DialogueIndex = 0;
        }
        Debug.Log("After prevConvo, DialogueIndex: " + DialogueIndex);
        x.TriggerDialogue("intro", DialogueIndex);
    }

     void EndDialogue()
    {
        DialogueIndex++;
        if (DialogueIndex <= DialogueLength)
        {
            x.TriggerDialogue("intro", DialogueIndex);
        }
        else 
        {
            gameObject.SetActive(false);
            UIMasterScript.enabled = true;
        }

    }

    public void SkipDialogue() 
    {
        DialogueIndex = DialogueLength;
        EndDialogue();
    }

    IEnumerator TypeSentence(string sentence) 
    {
        Char1Dialogue.text = "";
        Char2Dialogue.text = "";

        foreach (char letter in sentence.ToCharArray()) 
        {
            Char1Dialogue.text += letter;
            Char2Dialogue.text += letter;
            yield return .5f;
        }
    }

        public IEnumerator TriggerEndingDialogue()
    {
        x.TriggerDialogue("ending", 0);
        DialogueLength = x.endingDialogue.Length - 1;

        while (sentences.Count > 0)
        {
            yield return null;
        }

    }
}
