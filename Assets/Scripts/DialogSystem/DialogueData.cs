using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/DialogueData")]
public class DialogueData : ScriptableObject
{
    public LocalizedDialogueLine[] lines;
}