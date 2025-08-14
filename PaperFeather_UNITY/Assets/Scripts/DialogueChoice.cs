using UnityEngine;

[System.Serializable]
public class DialogueChoice
{
    public string choiceText;

    [Tooltip("Lignes jouées si ce choix est sélectionné. Peut être plusieurs.")]
    public DialogueLine[] branchLines;
}