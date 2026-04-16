using UnityEngine;
using System;
using InteractiveMuseum.Player;
using System.Collections;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [SerializeField] private DialogueData dialogue;
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private float triggerDelay = 0f;

    [Header("Optional Requirements")]
    [SerializeField] private string requiredItemId;
    [SerializeField] private bool consumeItemOnTrigger = false;

    private bool hasTriggered = false;
    private bool isInRange = false;

    public event Action OnDialogueTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (!CanTrigger()) return;

        if (other.CompareTag("Player"))
        {
            isInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = false;
        }
    }

    private void Update()
    {
        if (isInRange && CanTrigger() && Input.GetKeyDown(KeyCode.E))
        {
            TriggerDialogue();
        }
    }

    public void TriggerDialogue()
    {
        if (!CanTrigger()) return;

        PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();
        if (!string.IsNullOrEmpty(requiredItemId))
        {
            if (inventory == null || !inventory.HasItem(requiredItemId))
            {
                Debug.Log($"Cannot trigger dialogue: missing required item '{requiredItemId}'");
                return;
            }

            if (consumeItemOnTrigger)
            {
                inventory.ConsumeItem(requiredItemId);
            }
        }

        if (triggerDelay > 0)
        {
            Invoke(nameof(StartDialogueDelayed), triggerDelay);
        }
        else
        {
            StartDialogue();
        }
    }

    private void StartDialogueDelayed()
    {
        StartDialogue();
    }

    private void StartDialogue()
    {
        if (DialogueSystem.Instance == null)
        {
            Debug.LogError("DialogueSystem not found!");
            return;
        }

        DialogueSystem.Instance.StartDialogue(dialogue);

        if (triggerOnce)
            hasTriggered = true;

        OnDialogueTriggered?.Invoke();
    }

    private bool CanTrigger()
    {
        if (triggerOnce && hasTriggered) return false;
        if (DialogueSystem.Instance != null && DialogueSystem.Instance.IsDialogueActive()) return false;
        return true;
    }

    public void Interact()
    {
        if (CanTrigger())
        {
            TriggerDialogue();
        }
    }
}