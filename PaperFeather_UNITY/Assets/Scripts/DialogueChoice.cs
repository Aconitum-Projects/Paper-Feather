using UnityEngine;

[System.Serializable]
public class DialogueChoice
{
    public string choiceText;

    [Tooltip("Séquence jouée si ce choix est sélectionné.")]
    public DialogueSequence branchSequence;
}