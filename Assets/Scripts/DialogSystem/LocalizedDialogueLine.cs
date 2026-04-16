using System;
using UnityEngine;
using TMPro;
using UnityEngine.Localization.Components;
using System.Collections;

[Serializable]
public class LocalizedDialogueLine
{
    public string speakerNameKey;
    public string textKey;
    public string continueButtonKey;
    public string tableReference = "Localization Table";

    [NonSerialized] public string cachedSpeakerName;
    [NonSerialized] public string cachedText;
}