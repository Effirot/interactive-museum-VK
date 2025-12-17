using UnityEngine;

public class PickableObject : MonoBehaviour
{
    private Collider _collider;

    private Vector3 _destinationPoint;
    [HideInInspector]
    public bool placingActive;

    void Start()
    {
        _collider = GetComponent<Collider>();
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
        if (_collider != null)
        {
            _collider.enabled = false;
        }

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

        if (_collider != null)
        {
            _collider.enabled = true;
        }
    }

}
