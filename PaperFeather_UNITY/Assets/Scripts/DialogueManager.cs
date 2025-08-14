using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text speakerNameText;
    public TMP_Text dialogueText;
    public Image speakerImage;
    public GameObject choicesContainer;
    public GameObject choiceButtonPrefab;

    [Header("Settings")]
    public float typeSpeed = 0.03f;
    public KeyCode nextLineKey = KeyCode.R;

    private DialogueSequence currentSequence;
    private int currentLineIndex;

    // branch state
    private bool inBranch = false;
    private DialogueLine[] branchLines;
    private int branchIndex;
    private int returnMainIndex; // où revenir dans le main après la branch

    private bool isTyping;
    private Coroutine typingCoroutine;

    public void StartDialogue(DialogueSequence sequence)
    {
        currentSequence = sequence;
        currentLineIndex = 0;
        inBranch = false;
        DisplayCurrent();
    }

    void Update()
    {
        if (Input.GetKeyDown(nextLineKey))
            NextLine();
    }

    void DisplayCurrent()
    {
        DialogueLine line = GetCurrentLine();
        if (line == null) return;

        speakerNameText.text = line.speakerName;
        speakerImage.sprite = line.speakerSprite;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(line.text));

        if (line.hasChoices && line.choices != null && line.choices.Length > 0)
            ShowChoices(line.choices);
        else
            choicesContainer.SetActive(false);
    }

    DialogueLine GetCurrentLine()
    {
        if (!inBranch)
        {
            if (currentSequence == null || currentSequence.lines == null) return null;
            if (currentLineIndex < 0 || currentLineIndex >= currentSequence.lines.Length) return null;
            return currentSequence.lines[currentLineIndex];
        }
        else
        {
            if (branchLines == null) return null;
            if (branchIndex < 0 || branchIndex >= branchLines.Length) return null;
            return branchLines[branchIndex];
        }
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
        isTyping = false;
    }

    void ShowChoices(DialogueChoice[] choices)
    {
        foreach (Transform child in choicesContainer.transform)
            Destroy(child.gameObject);

        choicesContainer.SetActive(true);

        foreach (DialogueChoice choice in choices)
        {
            GameObject btnObj = Instantiate(choiceButtonPrefab, choicesContainer.transform);
            TMP_Text btnText = btnObj.GetComponentInChildren<TMP_Text>();
            btnText.text = choice.choiceText;

            Button btn = btnObj.GetComponent<Button>();

            // capture locale pour éviter problème de closure
            DialogueChoice captured = choice;
            btn.onClick.AddListener(() => OnChoiceSelected(captured));
        }
    }

    void OnChoiceSelected(DialogueChoice choice)
    {
        // stoppe typing en cours et affiche la première ligne de la branch immédiatement
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        isTyping = false;

        choicesContainer.SetActive(false);

        // sauvegarde où revenir dans le main (la ligne après celle qui contenait les choix)
        if (!inBranch)
            returnMainIndex = currentLineIndex + 1;
        else
            returnMainIndex = currentLineIndex + 1; // si nested, on simplifie: revient au main après la branch

        // lance la branch
        branchLines = choice.branchLines ?? new DialogueLine[0];
        inBranch = true;
        branchIndex = 0;

        if (branchLines.Length > 0)
            DisplayCurrent();
        else
            // si branch vide, on revient directement
            ExitBranch();
    }

    public void NextLine()
    {
        // si on tape pendant le typewriter => affiche tout
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            DialogueLine line = GetCurrentLine();
            if (line != null) dialogueText.text = line.text;
            isTyping = false;
            return;
        }

        if (inBranch)
        {
            branchIndex++;
            if (branchIndex < branchLines.Length)
            {
                DisplayCurrent();
            }
            else
            {
                ExitBranch();
            }
        }
        else
        {
            currentLineIndex++;
            if (currentSequence != null && currentLineIndex < currentSequence.lines.Length)
            {
                DisplayCurrent();
            }
            else
            {
                EndDialogue();
            }
        }
    }

    void ExitBranch()
    {
        inBranch = false;
        branchLines = null;
        branchIndex = 0;

        // on revient à la ligne suivante du main
        currentLineIndex = returnMainIndex;
        if (currentSequence != null && currentLineIndex < currentSequence.lines.Length)
            DisplayCurrent();
        else
            EndDialogue();
    }

    void EndDialogue()
    {
        Debug.Log("Dialogue terminé");
        // ferme UI / notify etc.
    }
}