using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public bool isSingleUse = false;
    public string onLookText = "Interact";
    public UnityEvent onInteract;
    bool singleUsed = false;
    public void interact(EntityPlayer player, Vector3 interactPoint)
    {
        if (!singleUsed)
            onInteract.Invoke();

        if (isSingleUse)
            singleUsed = true;
    }
}
