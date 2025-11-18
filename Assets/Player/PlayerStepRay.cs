using UnityEngine;

public class PlayerStepRay : MonoBehaviour
{
    public EntityLiving parentEntity;
    public float length;
    float initialDifference;
    void Start()
    {
        initialDifference = this.transform.localPosition.y;
    }

    private void FixedUpdate()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        int mask = 1 << 0;
        bool cast = Physics.Raycast(ray, out hit, length, mask);
        if (cast)
        {
            if (Vector3.Angle(hit.normal, Vector3.up) < parentEntity.stepAngle)
            {
                float difference = length - Vector3.Distance(transform.position, hit.point);
                Transform tr = parentEntity.transform;
                tr.position = new Vector3(tr.position.x, tr.position.y + difference - initialDifference, tr.position.z);
                parentEntity.entityRigidbody.linearVelocity = new Vector3(parentEntity.entityRigidbody.linearVelocity.x, 0, parentEntity.entityRigidbody.linearVelocity.z);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.greenYellow;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * length);
        Gizmos.DrawSphere(transform.position + transform.forward * length, 0.1F);
    }
}
