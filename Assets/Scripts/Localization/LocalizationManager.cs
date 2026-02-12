using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Purchasing;

public enum SystemLanguage
{
    English,
    Russian
}

public class LocalizationManager : MonoBehaviour
{
    [SerializeField] private SystemLanguage defaultLanguage = SystemLanguage.Russian;

    public static event Action OnManagerInitialized;
    public event Action OnLanguageChanged;

    public SystemLanguage CurrentLanguage { get; private set; }
    public static LocalizationManager Instance { get; private set; }

    private Dictionary<string, string> localizedTexts;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadSavedLanguage();
            LoadLocalization(CurrentLanguage);

            Debug.Log($"[LocalizationManager] Инициализирован с языком: {CurrentLanguage}");

            OnManagerInitialized?.Invoke();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadSavedLanguage()
    {
        int savedLang = PlayerPrefs.GetInt("GameLanguage", (int)defaultLanguage);
        CurrentLanguage = (SystemLanguage)savedLang;
    }

    private void LoadLocalization(SystemLanguage language)
    {
        localizedTexts = new Dictionary<string, string>();

        string fileName = language == SystemLanguage.English ? "Localization_English" : "Localization_Russian";

        TextAsset textAsset = Resources.Load<TextAsset>(fileName);

        if (textAsset == null)
        {
            textAsset = Resources.Load<TextAsset>($"Localization/{fileName}");
        }

        if (textAsset == null)
        {
            Debug.LogError($"[LocalizationManager] Файл локализации не найден: {fileName} в Resources/ или Resources/Localization/");
            return;
        }

        try
        {
            var jsonData = UnityEngine.Purchasing.MiniJSON.Json.Deserialize(textAsset.text) as Dictionary<string, object>;
            if (jsonData != null)
            {
                FlattenJson(jsonData, "");
                Debug.Log($"[LocalizationManager] Загружено {localizedTexts.Count} ключей для {fileName}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[LocalizationManager] Ошибка загрузки локализации: {e.Message}");
        }
    }

    private void FlattenJson(Dictionary<string, object> data, string prefix)
    {
        foreach (var pair in data)
        {
            string key = string.IsNullOrEmpty(prefix) ? pair.Key : $"{prefix}.{pair.Key}";

            if (pair.Value is Dictionary<string, object> nestedDict)
            {
                FlattenJson(nestedDict, key);
            }
            else
            {
                localizedTexts[key] = pair.Value.ToString();
            }
        }
    }

    public string GetLocalizedText(string key)
    {
        if (localizedTexts == null)
        {
            Debug.LogWarning($"[LocalizationManager] Локализация не загружена, возвращаю ключ: {key}");
            return key;
        }

        if (localizedTexts.ContainsKey(key))
            return localizedTexts[key];

        Debug.LogWarning($"[LocalizationManager] Ключ не найден: {key}");
        return key;
    }

    public void SetLanguage(SystemLanguage language)
    {
        if (CurrentLanguage == language)
            return;

        CurrentLanguage = language;
        PlayerPrefs.SetInt("GameLanguage", (int)language);
        PlayerPrefs.Save();

        LoadLocalization(language);
        OnLanguageChanged?.Invoke();

        Debug.Log($"[LocalizationManager] Язык изменен на: {language}");
    }
}