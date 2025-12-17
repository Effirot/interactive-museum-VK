using UnityEngine;

public class PickableObject : MonoBehaviour
{
    Collider collider;

    Vector3 destinationPoint;
    [HideInInspector]
    public bool placingActive;

    void Start()
    {
        collider = GetComponent<Collider>();
        // this.gameObject.layer = 0;
    }

    private void Update()
    {
        if (placingActive)
        {
            this.transform.position = MyUtils.Follow(this.transform.position, destinationPoint, 0.05F);
            MyUtils.SetMove(this.gameObject, 0, 0, 0);

            if (this.transform.position.Equals(destinationPoint))
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
        if (collider != null)
        {
            collider.enabled = false;
        }

    }
    public void OnPlace(Vector3 destinationPoint)
    {
        this.destinationPoint = destinationPoint;
        this.placingActive = true;
        OnPlaceOrDrop();
    }
    public void OnDrop()
    {
        OnPlaceOrDrop();
    }

    public void OnPlaceOrDrop()
    {

        if (collider != null)
        {
            collider.enabled = true;
        }
    }

}
