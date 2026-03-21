using InteractiveMuseum.Player;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public bool isSingleUse = false;
    public string onLookText = "Interact";
    
    [Header("Inventory Requirement")]
    [SerializeField]
    private bool requiresInventoryItem = false;
    [SerializeField]
    private string requiredItemId = "";
    [SerializeField]
    private bool consumeRequiredItem = true;
    [SerializeField]
    private string missingItemText = "Need a required item";
    [SerializeField]
    private UnityEvent onMissingRequiredItem;
    public UnityEvent onInteract;

    bool singleUsed = false;
    public void Interact(PlayerMovementController player, Vector3 interactPoint)
    {
        if (!CanInteract(player))
            return;

        if (!singleUsed)
            onInteract.Invoke();

        if (isSingleUse)
            singleUsed = true;
    }
    
    // Legacy support for EntityPlayer
    public void Interact(EntityPlayer player, Vector3 interactPoint)
    {
        if (!CanInteract(null))
            return;

        if (!singleUsed)
            onInteract.Invoke();

        if (isSingleUse)
            singleUsed = true;
    }

    private bool CanInteract(PlayerMovementController player)
    {
        if (!requiresInventoryItem || string.IsNullOrWhiteSpace(requiredItemId))
            return true;

        if (player == null || player.Inventory == null)
            return false;

        bool hadItem;
        if (consumeRequiredItem)
        {
            hadItem = player.Inventory.ConsumeItem(requiredItemId);
        }
        else
        {
            hadItem = player.Inventory.HasItem(requiredItemId);
        }

        if (hadItem)
            return true;

        CanvasManager canvasManager = FindFirstObjectByType<CanvasManager>();
        if (canvasManager != null)
        {
            canvasManager.setInteractionInfo(missingItemText);
        }
        onMissingRequiredItem?.Invoke();
        return false;
    }
}
