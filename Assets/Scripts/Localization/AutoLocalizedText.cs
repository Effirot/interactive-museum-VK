using UnityEngine;
using TMPro;
using UnityEngine.Localization.Components;

[RequireComponent(typeof(TextMeshProUGUI))]
public class AutoLocalizedText : MonoBehaviour
{
    [SerializeField] private string tableReference = "Localization Table";
    [SerializeField] private string entryKey;

    private LocalizeStringEvent localizeStringEvent;
    private TextMeshProUGUI textComponent;

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        localizeStringEvent = GetComponent<LocalizeStringEvent>();

        if (localizeStringEvent == null)
        {
            localizeStringEvent = gameObject.AddComponent<LocalizeStringEvent>();
        }

        localizeStringEvent.StringReference.TableReference = tableReference;
        localizeStringEvent.StringReference.TableEntryReference = entryKey;
        localizeStringEvent.StringReference.Arguments = null;

        localizeStringEvent.OnUpdateString.AddListener(UpdateText);
    }

    private void UpdateText(string localizedText)
    {
        if (textComponent != null)
        {
            textComponent.text = localizedText;
        }
    }

    public void SetKey(string newKey)
    {
        entryKey = newKey;
        localizeStringEvent.StringReference.TableEntryReference = entryKey;
        localizeStringEvent.RefreshString();
    }

    private void OnDestroy()
    {
        if (localizeStringEvent != null)
        {
            localizeStringEvent.OnUpdateString.RemoveListener(UpdateText);
        }
    }
}