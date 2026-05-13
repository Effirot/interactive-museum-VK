using UnityEngine;
using UnityEngine.VFX;

public class WaterTap : MonoBehaviour
{
    [Header("Water Effect")]
    [SerializeField]
    private GameObject waterVFX;

    [SerializeField]
    private AudioSource waterSound;

    [SerializeField]
    private bool isOpen = false;

    public void OpenTap()
    {
        if (isOpen) return;
        isOpen = true;

        if (waterVFX != null)
        {
            waterVFX.SetActive(true);
        }

        if (waterSound != null)
            waterSound.Play();
    }

    public void CloseTap()
    {
        if (!isOpen) return;
        isOpen = false;


        if (waterSound != null)
            waterSound.Stop();
    }

    public void ToggleTap()
    {
        if (isOpen) CloseTap();
        else OpenTap();
    }
}