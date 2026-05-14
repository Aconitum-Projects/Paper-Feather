using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text speakerNameText;
    public TMP_Text dialogueText;
    public Image speakerImage;
    public GameObject choicesContainer;
    public GameObject choiceButtonPrefab;
    
    [Header("Panels")]
    public CanvasGroup namePanel;
    public CanvasGroup dialoguePanel;
    public Ease animEase = Ease.OutBack;
    
    [Header("Settings")]
    public float typeSpeed = 0.03f;
    public float fadeDuration = 0.3f;
    public Key nextLineKey = Key.R;

    private DialogueSequence currentSequence;
    private int currentLineIndex;

    // branch state
    private bool inBranch = false;
    private DialogueLine[] branchLines;
    private int branchIndex;
    private int returnMainIndex;

    private bool isTyping;
    private Coroutine typingCoroutine;
    private Sprite previousSprite;

    void Start()
    {
        if (namePanel != null)
        {
            namePanel.alpha = 0f;
            namePanel.interactable = false;
            namePanel.blocksRaycasts = false;
        }
        if (dialoguePanel != null)
        {
            dialoguePanel.alpha = 0f;
            dialoguePanel.interactable = false;
            dialoguePanel.blocksRaycasts = false;
        }

        if (speakerImage != null)
        {
            speakerImage.rectTransform.localScale = new Vector3(1f, 0f, 1f);
        }
    }
    
    public void StartDialogue(DialogueSequence sequence)
    {
        currentSequence = sequence;
        currentLineIndex = 0;
        inBranch = false;

        Vector2 offscreenPos = new Vector2(-4000f, 0f);
        if (namePanel != null)
        {
            namePanel.alpha = 0f;
            namePanel.interactable = false;
            namePanel.blocksRaycasts = false;
            namePanel.GetComponent<RectTransform>().anchoredPosition = offscreenPos;

            Sequence nameSeq = DOTween.Sequence();
            nameSeq.Append(namePanel.DOFade(1f, fadeDuration));
            nameSeq.Join(namePanel.GetComponent<RectTransform>().DOAnchorPos(Vector2.zero, fadeDuration).SetEase(animEase));
            nameSeq.OnStart(() =>
            {
                namePanel.interactable = true;
                namePanel.blocksRaycasts = true;
            });
            nameSeq.Play();
        }
        if (dialoguePanel != null)
        {
            dialoguePanel.alpha = 0f;
            dialoguePanel.interactable = false;
            dialoguePanel.blocksRaycasts = false;
            dialoguePanel.GetComponent<RectTransform>().anchoredPosition = offscreenPos;

            Sequence dialogueSeq = DOTween.Sequence();
            dialogueSeq.Append(dialoguePanel.DOFade(1f, fadeDuration));
            dialogueSeq.Join(dialoguePanel.GetComponent<RectTransform>().DOAnchorPos(Vector2.zero, fadeDuration).SetEase(animEase));
            dialogueSeq.OnStart(() =>
            {
                dialoguePanel.interactable = true;
                dialoguePanel.blocksRaycasts = true;
            });
            dialogueSeq.Play();
        }

        DisplayCurrent();
    }

    void Update()
    {
        if (IsKeyPressedThisFrame(nextLineKey))
            NextLine();
    }

    void DisplayCurrent()
    {
        DialogueLine line = GetCurrentLine();
        if (line == null) return;

        speakerNameText.text = line.speakerName;

        if (speakerImage.sprite != line.speakerSprite)
        {
            speakerImage.sprite = line.speakerSprite;

            RectTransform spriteRect = speakerImage.GetComponent<RectTransform>();
            spriteRect.localScale = new Vector3(1f, 0f, 1f);
            speakerImage.color = new Color(1f, 1f, 1f, 1f);

            spriteRect.DOScaleY(1f, fadeDuration).SetEase(animEase);
        }

        previousSprite = line.speakerSprite;

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

        bool insideTag = false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            if (c == '<')
            {
                insideTag = true;
            }
            if (insideTag)
            {
                dialogueText.text += c;
                if (c == '>')
                {
                    insideTag = false;
                }
                continue;
            }

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

            DialogueChoice captured = choice;
            btn.onClick.AddListener(() => OnChoiceSelected(captured));
        }
    }

    void OnChoiceSelected(DialogueChoice choice)
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        isTyping = false;

        choicesContainer.SetActive(false);

        if (!inBranch)
            returnMainIndex = currentLineIndex + 1;
        else
            returnMainIndex = currentLineIndex + 1;

        branchLines = choice.branchSequence != null ? choice.branchSequence.lines : new DialogueLine[0];
        inBranch = true;
        branchIndex = 0;

        if (branchLines.Length > 0)
            DisplayCurrent();
        else
            ExitBranch();
    }

    public void NextLine()
    {
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

        currentLineIndex = returnMainIndex;
        if (currentSequence != null && currentLineIndex < currentSequence.lines.Length)
            DisplayCurrent();
        else
            EndDialogue();
    }

    void EndDialogue()
    {
        fadeDuration *= 2f;
        
        Debug.Log("Dialogue terminé");

        Vector2 offscreenPos = new Vector2(-4000f, 0f);

        if (namePanel != null)
        {
            Sequence nameSeq = DOTween.Sequence();
            nameSeq.Append(namePanel.DOFade(0f, fadeDuration));
            nameSeq.Join(namePanel.GetComponent<RectTransform>().DOAnchorPos(offscreenPos, fadeDuration).SetEase(animEase));
            nameSeq.OnComplete(() =>
            {
                namePanel.interactable = false;
                namePanel.blocksRaycasts = false;
            });
            nameSeq.Play();
        }

        if (dialoguePanel != null)
        {
            Sequence dialogueSeq = DOTween.Sequence();
            dialogueSeq.Append(dialoguePanel.DOFade(0f, fadeDuration));
            dialogueSeq.Join(dialoguePanel.GetComponent<RectTransform>().DOAnchorPos(offscreenPos, fadeDuration).SetEase(animEase));
            dialogueSeq.OnComplete(() =>
            {
                dialoguePanel.interactable = false;
                dialoguePanel.blocksRaycasts = false;
            });
            dialogueSeq.Play();
        }

        if (speakerImage != null)
        {
            RectTransform spriteRect = speakerImage.GetComponent<RectTransform>();
    
            Sequence spriteSeq = DOTween.Sequence();
            spriteSeq.Append(speakerImage.DOFade(0f, fadeDuration));
            spriteSeq.Join(spriteRect.DOScaleY(0f, fadeDuration).SetEase(animEase));
            spriteSeq.Play();
        }

        if (choicesContainer != null)
            choicesContainer.SetActive(false);
        
        fadeDuration /= 2f;
    }

    bool IsKeyPressedThisFrame(Key key)
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return false;

        var keyControl = keyboard[key];
        return keyControl != null && keyControl.wasPressedThisFrame;
    }
}