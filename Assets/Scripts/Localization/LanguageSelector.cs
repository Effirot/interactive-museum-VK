using UnityEngine;
using UnityEngine.Localization.Settings;
using TMPro;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(TMP_Dropdown))]
public class LanguageSelector : MonoBehaviour
{
    private TMP_Dropdown dropdown;
    private bool isInitialized = false;

    private void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
    }

    private void Start()
    {
        StartCoroutine(InitializeDropdown());
    }

    private System.Collections.IEnumerator InitializeDropdown()
    {
        yield return LocalizationSettings.InitializationOperation;

        dropdown.ClearOptions();

        var options = new List<TMP_Dropdown.OptionData>();

        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            options.Add(new TMP_Dropdown.OptionData(locale.LocaleName));
        }

        dropdown.AddOptions(options);

        int currentIndex = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);
        dropdown.value = currentIndex;
        dropdown.RefreshShownValue();

        dropdown.onValueChanged.AddListener(OnLanguageChanged);
        isInitialized = true;
    }

    private void OnLanguageChanged(int index)
    {
        if (!isInitialized) return;

        var selectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        LocalizationSettings.SelectedLocale = selectedLocale;
    }

    private void OnDestroy()
    {
        if (dropdown != null && isInitialized)
        {
            dropdown.onValueChanged.RemoveListener(OnLanguageChanged);
        }
    }
}