using InteractiveMuseum.Player;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public bool isSingleUse = false;
    public string onLookText = "Interact";
    public UnityEvent onInteract;
    bool singleUsed = false;
    public void Interact(PlayerMovementController player, Vector3 interactPoint)
    {
        if (!singleUsed)
            onInteract.Invoke();

        if (isSingleUse)
            singleUsed = true;
    }
    
    // Legacy support for EntityPlayer
    public void Interact(EntityPlayer player, Vector3 interactPoint)
    {
        if (!singleUsed)
            onInteract.Invoke();

        if (isSingleUse)
            singleUsed = true;
    }
}
