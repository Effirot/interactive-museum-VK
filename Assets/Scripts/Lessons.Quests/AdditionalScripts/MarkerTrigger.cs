using UnityEngine;
using UnityEngine.Events;

public class MarkerTrigger : MonoBehaviour
{
    [SerializeField]
    private UnityEvent onMarkerActivated;

    private void OnEnable()
    {
        onMarkerActivated.Invoke();
    }
}