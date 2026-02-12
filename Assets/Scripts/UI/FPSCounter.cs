using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    public static FPSCounter Instance { get; private set; }

    public bool showFPS = true;

    public int fontSize = 20;
    public Color textColor = Color.white;
    public Vector2 positionOffset = new Vector2(0, 0);

    private Text fpsText;
    private Canvas fpsCanvas;
    private float fps;
    private float deltaTime = 0.0f;
    private const string FPS_PREF_KEY = "ShowFPS";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();

        CreateFPSUI();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        EnsureFPSUIExists();

        UpdateFPSVisibility();

        ForceUpdateFPSText();
    }

    private void CreateFPSUI()
    {
        if (fpsCanvas == null)
        {
            CreateCanvas();
        }

        if (fpsText == null)
        {
            CreateText();
        }
    }

    private void EnsureFPSUIExists()
    {
        bool needRecreate = false;

        if (fpsCanvas == null || fpsText == null)
        {
            needRecreate = true;
        }
        else if (fpsText != null && !fpsText.IsDestroyed() && fpsText.name != "FPS_Text")
        {
            needRecreate = true;
        }

        if (needRecreate)
        {
            Debug.Log("FPS UI elements missing or corrupted, recreating...");
            DestroyOldUI();
            CreateFPSUI();
        }
    }

    private void DestroyOldUI()
    {
        if (fpsText != null && fpsText.gameObject != null)
        {
            Destroy(fpsText.gameObject);
        }

        if (fpsCanvas != null && fpsCanvas.gameObject != null)
        {
            Destroy(fpsCanvas.gameObject);
        }

        fpsText = null;
        fpsCanvas = null;
    }

    private void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("FPS_Canvas");
        fpsCanvas = canvasObj.AddComponent<Canvas>();
        fpsCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fpsCanvas.sortingOrder = 9999; 

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        canvasObj.transform.SetParent(transform);

        Debug.Log("FPS Canvas created");
    }

    private void CreateText()
    {
        if (fpsCanvas == null) return;

        GameObject textObj = new GameObject("FPS_Text");
        textObj.transform.SetParent(fpsCanvas.transform);

        fpsText = textObj.AddComponent<Text>();
        fpsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        fpsText.fontSize = fontSize;
        fpsText.color = textColor;
        fpsText.alignment = TextAnchor.UpperRight;

        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(1, 1);
        rectTransform.anchoredPosition = positionOffset;
        rectTransform.sizeDelta = new Vector2(150, 30);

        fpsText.text = "FPS: --";

        Debug.Log("FPS Text created");
    }

    private void Update()
    {
        CalculateFPS();

        if (showFPS && fpsText != null)
        {
            UpdateFPSText();
        }
    }

    private void CalculateFPS()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        fps = 1.0f / deltaTime;
    }

    private void UpdateFPSText()
    {
        if (fpsText == null) return;

        fpsText.text = $"FPS: {Mathf.RoundToInt(fps)}";
    }

    private void ForceUpdateFPSText()
    {
        if (fpsText != null)
        {
            fpsText.text = $"FPS: {Mathf.RoundToInt(fps)}";
            Debug.Log($"Force updated FPS text to: {Mathf.RoundToInt(fps)}");
        }
    }

    private void UpdateFPSVisibility()
    {
        if (fpsText != null)
        {
            fpsText.gameObject.SetActive(showFPS);

            if (showFPS)
            {
                ForceUpdateFPSText();
            }
        }

        if (fpsCanvas != null)
        {
            fpsCanvas.gameObject.SetActive(showFPS);
        }
    }

    public void ToggleFPS(bool value)
    {
        showFPS = value;
        PlayerPrefs.SetInt(FPS_PREF_KEY, value ? 1 : 0);
        PlayerPrefs.Save();

        UpdateFPSVisibility();
    }

    private void LoadSettings()
    {
        showFPS = PlayerPrefs.GetInt(FPS_PREF_KEY, 1) == 1;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");

        StartCoroutine(ReinitializeUIAfterSceneLoad());
    }

    private System.Collections.IEnumerator ReinitializeUIAfterSceneLoad()
    {
        yield return new WaitForEndOfFrame();

        EnsureFPSUIExists();
        UpdateFPSVisibility();
        ForceUpdateFPSText();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
