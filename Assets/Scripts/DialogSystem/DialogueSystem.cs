using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using InteractiveMuseum.Player;
using UnityEngine.Localization.Settings;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameObject continueButton;
    [SerializeField] private GameObject canvasInventory;
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float scrollSensitivity = 20f;

    [Header("Settings")]
    [SerializeField] private bool autoStart = false;

    private DialogueData currentDialogue;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    public bool endDialogue = false;
    private Coroutine typingCoroutine;
    private string currentFullText;
    private bool isDialogueActive = false;

    private PlayerInput playerInput;
    private InputAction interactAction;
    private InputAction scrollAction;

    private Vector2 mouseScrollDelta;

    public UnityEvent OnDialogueStart;
    public UnityEvent OnDialogueEnd;
    public static DialogueSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        SetupInputSystem();
    }

    private void SetupInputSystem()
    {
        playerInput = FindFirstObjectByType<PlayerInput>();

        if (playerInput != null)
        {
            interactAction = playerInput.actions["Interact"];
            if (interactAction == null)
            {
                Debug.LogWarning("Interact action not found in Input Actions, will use keyboard detection");
            }
        }

        scrollAction = new InputAction("Scroll", binding: "<Mouse>/scroll");
        scrollAction.Enable();
    }

    private void OnEnable()
    {
        UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDisable()
    {
        UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(UnityEngine.Localization.Locale locale)
    {
        if (isDialogueActive && currentDialogue != null && currentLineIndex < currentDialogue.lines.Length)
        {
            RefreshCurrentLine();
        }
    }

    private void Update()
    {
        if (!isDialogueActive) return;

        if (scrollAction != null)
        {
            Vector2 scrollValue = scrollAction.ReadValue<Vector2>();
            if (scrollValue.y != 0 && scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition += scrollValue.y * scrollSensitivity * Time.deltaTime;
            }
        }

        bool continuePressed = false;

        if (interactAction != null)
        {
            continuePressed = interactAction.WasPressedThisFrame();
        }

        if (!continuePressed)
        {
            continuePressed = Keyboard.current != null &&
                (Keyboard.current.eKey.wasPressedThisFrame ||
                 Keyboard.current.spaceKey.wasPressedThisFrame);

            if (!continuePressed && Mouse.current != null)
            {
                continuePressed = Mouse.current.leftButton.wasPressedThisFrame;
            }
        }

        if (continuePressed)
        {
            if (isTyping)
            {
                if (typingCoroutine != null)
                {
                    StopCoroutine(typingCoroutine);
                    typingCoroutine = null;
                }
                dialogueText.text = currentFullText;
                isTyping = false;
                if (continueButton != null)
                    continueButton.SetActive(true);
            }
            else
            {
                NextLine();
            }
        }
    }

    public void StartDialogue(DialogueData dialogue)
    {
        if (dialogue == null || dialogue.lines == null || dialogue.lines.Length == 0)
        {
            Debug.LogWarning("Cannot start empty dialogue");
            return;
        }

        if (canvasInventory == null)
        {
            GameObject inventory = GameObject.Find("Inventory");
            if (inventory != null)
            {
                canvasInventory = inventory;
            }
            else
            {
                Debug.LogWarning("CanvasInventory not found in scene!");
            }
        }

        canvasInventory.SetActive(false);
        currentDialogue = dialogue;
        currentLineIndex = 0;
        isDialogueActive = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        DisablePlayerControl(true);

        OnDialogueStart?.Invoke();

        DisplayCurrentLine();
    }

    private string GetLocalizedString(string tableReference, string entryKey)
    {
        if (string.IsNullOrEmpty(entryKey))
            return "";

        var table = LocalizationSettings.StringDatabase.GetTable(tableReference);
        if (table != null)
        {
            var entry = table.GetEntry(entryKey);
            if (entry != null)
                return entry.GetLocalizedString();
        }

        Debug.LogWarning($"Localization key not found: {entryKey} in table {tableReference}");
        return entryKey;
    }

    private void DisplayCurrentLine()
    {
        if (currentLineIndex >= currentDialogue.lines.Length)
        {
            EndDialogue();
            return;
        }

        LocalizedDialogueLine line = currentDialogue.lines[currentLineIndex];

        string localizedSpeakerName = GetLocalizedString(line.tableReference, line.speakerNameKey);
        if (speakerNameText != null)
            speakerNameText.text = localizedSpeakerName;

        string localizedText = GetLocalizedString(line.tableReference, line.textKey);
        currentFullText = localizedText;

        line.cachedSpeakerName = localizedSpeakerName;
        line.cachedText = localizedText;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(localizedText));

        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;
    }

    private void RefreshCurrentLine()
    {
        if (currentLineIndex >= currentDialogue.lines.Length) return;

        LocalizedDialogueLine line = currentDialogue.lines[currentLineIndex];

        string localizedSpeakerName = GetLocalizedString(line.tableReference, line.speakerNameKey);
        if (speakerNameText != null)
            speakerNameText.text = localizedSpeakerName;

        string localizedText = GetLocalizedString(line.tableReference, line.textKey);

        if (!isTyping)
        {
            dialogueText.text = localizedText;
            currentFullText = localizedText;
        }
        else
        {
            currentFullText = localizedText;
        }

        line.cachedSpeakerName = localizedSpeakerName;
        line.cachedText = localizedText;
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        if (continueButton != null)
            continueButton.SetActive(false);

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        if (continueButton != null)
            continueButton.SetActive(true);
    }

    private void NextLine()
    {
        if (!isDialogueActive) return;

        currentLineIndex++;
        DisplayCurrentLine();
    }

    private void EndDialogue()
    {
        if (canvasInventory == null)
        {
            GameObject inventory = GameObject.Find("Inventory");
            if (inventory != null)
            {
                canvasInventory = inventory;
            }
        }
        canvasInventory.SetActive(true);
        isDialogueActive = false;
        endDialogue = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        DisablePlayerControl(false);

        PlayerMovementController player = FindFirstObjectByType<PlayerMovementController>();
        if (player != null && !player.isInMiniGameMode)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (player == null)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        OnDialogueEnd?.Invoke();
    }

    private void DisablePlayerControl(bool disable)
    {
        PlayerMovementController player = FindFirstObjectByType<PlayerMovementController>();
        if (player != null)
        {
            player.enabled = !disable;
        }
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    public void ForceEndDialogue()
    {
        if (isDialogueActive)
        {
            EndDialogue();
        }
    }

    private void OnDestroy()
    {
        if (interactAction != null)
        {
            interactAction.Disable();
            interactAction.Dispose();
        }

        if (scrollAction != null)
        {
            scrollAction.Disable();
            scrollAction.Dispose();
        }
    }
}