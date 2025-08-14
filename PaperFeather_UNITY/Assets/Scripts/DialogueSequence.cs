using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Visual Novel/Dialogue Sequence")]
public class DialogueSequence : ScriptableObject
{
    public DialogueLine[] lines;
}