using UnityEngine;

public class Roach : MonoBehaviour
{

    public Vector3 moveDirection;

    public float speed = 0.03F;


    void Start()
    {
        moveDirection = new Vector3((float)MyUtils.rand.NextDouble() - 0.5F, 0, (float)MyUtils.rand.NextDouble() - 0.5F).normalized * speed;
    }

    void FixedUpdate()
    {
        this.transform.position += moveDirection;
        RaycastHit hit = MyUtils.fixedRaycast(this.transform.position, moveDirection, 0.1F, 1, 0);
        if (hit.collider != null)
        {
            moveDirection = Vector3.Reflect(moveDirection, hit.normal);
            moveDirection.y = 0;
        }



    }

    private void OnTriggerEnter(Collider other)
    {
        PickableObject po = other.gameObject.GetComponent<PickableObject>();
        if (po != null && po.placingActive)
        {
            //бесконечный спаун
            //for (int i = 0; i < 4; i++)
            //{
            //    GameObject go = Instantiate(this.gameObject);
            //    go.transform.localScale *= 0.8F;
            //}



            Destroy(this.gameObject);

        }
    }
}
