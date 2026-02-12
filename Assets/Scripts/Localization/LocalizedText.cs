using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string localizationKey;

    private TextMeshProUGUI textComponent;
    private bool isSubscribed = false;
    private bool isInitialized = false;

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();

        if (string.IsNullOrEmpty(localizationKey))
        {
            localizationKey = textComponent.text;
            Debug.LogWarning($"[LocalizedText] Ключ не задан для {gameObject.name}, использую текст как ключ: '{localizationKey}'");
        }
    }

    private void Start()
    {
        TryInitialize();
    }

    private void OnEnable()
    {
        TryInitialize();
    }

    private void TryInitialize()
    {
        if (isInitialized) return;

        if (LocalizationManager.Instance == null)
        {
            Debug.Log($"[LocalizedText] Менеджер еще не инициализирован для {gameObject.name}, ждем...");

            LocalizationManager.OnManagerInitialized += OnManagerInitialized;
            return;
        }

        CompleteInitialization();
    }

    private void OnManagerInitialized()
    {
        LocalizationManager.OnManagerInitialized -= OnManagerInitialized;
        CompleteInitialization();
    }

    private void CompleteInitialization()
    {
        if (isInitialized) return;

        if (LocalizationManager.Instance == null)
        {
            Debug.LogError($"[LocalizedText] LocalizationManager все еще не найден! {gameObject.name}");
            return;
        }

        if (!isSubscribed)
        {
            LocalizationManager.Instance.OnLanguageChanged += UpdateTranslation;
            isSubscribed = true;
        }

        UpdateTranslation();
        isInitialized = true;

        Debug.Log($"[LocalizedText] Успешно инициализирован: {gameObject.name}");
    }

    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null && isSubscribed)
        {
            LocalizationManager.Instance.OnLanguageChanged -= UpdateTranslation;
        }
        LocalizationManager.OnManagerInitialized -= OnManagerInitialized;
    }

    private void UpdateTranslation()
    {
        if (textComponent == null) return;

        string translatedText = LocalizationManager.Instance.GetLocalizedText(localizationKey);
        textComponent.text = translatedText;
    }

    public void SetLocalizationKey(string newKey)
    {
        localizationKey = newKey;
        if (isInitialized)
        {
            UpdateTranslation();
        }
    }
}