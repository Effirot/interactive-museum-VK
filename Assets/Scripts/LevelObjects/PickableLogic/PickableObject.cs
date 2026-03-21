using UnityEngine;

public class PickableObject : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField]
    private string itemId = "item";

    [SerializeField]
    private string itemDisplayName = "Item";

    [SerializeField]
    private Sprite itemIcon;

    private Collider _collider;
    private Collider[] _allColliders;
    private Renderer[] _allRenderers;
    private Rigidbody _rigidbody;

    private Vector3 _destinationPoint;
    [HideInInspector]
    public bool placingActive;

    void Start()
    {
        _collider = GetComponent<Collider>();
        _allColliders = GetComponentsInChildren<Collider>(true);
        _allRenderers = GetComponentsInChildren<Renderer>(true);
        _rigidbody = GetComponent<Rigidbody>();
        // this.gameObject.layer = 0;
    }

    private void Update()
    {
        if (placingActive)
        {
            transform.position = MyUtils.Follow(transform.position, _destinationPoint, 0.05F);
            MyUtils.SetMove(gameObject, 0, 0, 0);

            if (transform.position.Equals(_destinationPoint))
            {
                placingActive = false;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 0)
        placingActive = false;

    }

    public void OnPick()
    {
        SetWorldVisible(false);
    }
    public void OnPlace(Vector3 destinationPoint)
    {
        _destinationPoint = destinationPoint;
        placingActive = true;
        OnPlaceOrDrop();
    }
    public void OnDrop()
    {
        OnPlaceOrDrop();
    }

    public void OnPlaceOrDrop()
    {
        SetWorldVisible(true);
    }

    public string ItemId => itemId;
    public string ItemDisplayName => itemDisplayName;
    public Sprite ItemIcon => itemIcon;

    private void SetWorldVisible(bool visible)
    {
        if (_allColliders == null || _allColliders.Length == 0)
        {
            _allColliders = GetComponentsInChildren<Collider>(true);
        }

        if (_allRenderers == null || _allRenderers.Length == 0)
        {
            _allRenderers = GetComponentsInChildren<Renderer>(true);
        }

        foreach (var c in _allColliders)
        {
            if (c != null)
            {
                c.enabled = visible;
            }
        }

        foreach (var r in _allRenderers)
        {
            if (r != null)
            {
                r.enabled = visible;
            }
        }

        if (_rigidbody == null)
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        if (_rigidbody != null)
        {
            _rigidbody.isKinematic = !visible;
            if (!visible)
            {
                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
            }
        }
    }

}
