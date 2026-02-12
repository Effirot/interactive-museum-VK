using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public Toggle fpsToggle;

    private void Start()
    {
        EnsureFPSCounterExists();

        SetupUI();

    }

    private void EnsureFPSCounterExists()
    {
        if (FPSCounter.Instance == null)
        {
            GameObject fpsObj = new GameObject("FPSCounter");
            fpsObj.AddComponent<FPSCounter>();
            Debug.Log("Created FPSCounter in Menu");
        }
    }

    private void SetupUI()
    {
        if (fpsToggle != null)
        {
            bool showFPS = PlayerPrefs.GetInt("ShowFPS", 1) == 1;
            fpsToggle.isOn = showFPS;

            if (FPSCounter.Instance != null)
            {
                FPSCounter.Instance.ToggleFPS(showFPS);
            }

            fpsToggle.onValueChanged.AddListener(OnFPSToggleChanged);
        }
    }

    public void OnPlayClicked()
    {
        SceneManager.LoadScene("SampleScene");
    }

    private void OnFPSToggleChanged(bool value)
    {
        PlayerPrefs.SetInt("ShowFPS", value ? 1 : 0);
        PlayerPrefs.Save();

        if (FPSCounter.Instance != null)
        {
            FPSCounter.Instance.ToggleFPS(value);
        }
    }
}
