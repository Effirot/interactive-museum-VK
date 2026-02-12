using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Dropdown))]
public class LanguageDropdownController : MonoBehaviour
{
    private TMP_Dropdown dropdown;

    private void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
    }

    private void Start()
    {
        if (LocalizationManager.Instance == null)
        {
            Debug.LogError("LocalizationManager не найден!");
            return;
        }

        InitializeDropdown();

        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
    }

    private void OnDestroy()
    {
        if (dropdown != null)
            dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);

        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
    }

    private void InitializeDropdown()
    {
        dropdown.ClearOptions();

        var options = new List<TMP_Dropdown.OptionData>();

        string englishName = LocalizationManager.Instance.GetLocalizedText("English");
        options.Add(new TMP_Dropdown.OptionData(englishName));

        string russianName = LocalizationManager.Instance.GetLocalizedText("Русский");
        options.Add(new TMP_Dropdown.OptionData(russianName));

        dropdown.AddOptions(options);

        dropdown.value = (int)LocalizationManager.Instance.CurrentLanguage;
        dropdown.RefreshShownValue();
    }

    private void OnDropdownValueChanged(int index)
    {
        SystemLanguage selectedLanguage = (SystemLanguage)index;
        LocalizationManager.Instance.SetLanguage(selectedLanguage);
    }

    private void OnLanguageChanged()
    {
        var options = dropdown.options;

        options[0].text = LocalizationManager.Instance.GetLocalizedText("English");
        options[1].text = LocalizationManager.Instance.GetLocalizedText("Русский");

        dropdown.RefreshShownValue();

        dropdown.value = (int)LocalizationManager.Instance.CurrentLanguage;
    }
}
