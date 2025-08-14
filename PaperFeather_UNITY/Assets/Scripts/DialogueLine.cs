using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(2,5)] public string text;
    public Sprite speakerSprite;

    [Header("Choices (optionnel)")]
    public bool hasChoices;
    public DialogueChoice[] choices;
}