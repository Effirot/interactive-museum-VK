using System;
using UnityEngine;

[Serializable]
public class DialogueLine
{
    [Tooltip("Localization key for speaker name")]
    public string speakerNameKey;

    [Tooltip("Localization key for dialogue text")]
    public string textKey;
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/DialogueData")]
public class DialogueData : ScriptableObject
{
    public DialogueLine[] lines;

    [Header("Optional Triggers")]
    public string onCompleteTriggerId;
    public bool autoDestroyOnComplete = false;
}