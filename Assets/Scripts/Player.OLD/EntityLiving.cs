
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class EntityLiving : MonoBehaviour
{
    [HideInInspector]
    public Rigidbody entityRigidbody;
    [HideInInspector]
    public int ticksExisted;
    public bool isDead;
    public bool onGround;
    public UserInterface userInterface;
    public float stepAngle;

    // Start is called before the first frame update
    public virtual void Start()
    {
        entityRigidbody = this.GetComponent<Rigidbody>();
    }

    public virtual Vector3 GetVelocity()
    {
        if (entityRigidbody != null)
        return entityRigidbody.linearVelocity;
        else return Vector3.zero;
    }

    // Update is called once per frame
    public virtual void Update()
    {
        
    }

    public void FixedUpdate()
    {
        ticksExisted++;
        updateEntity();
        onGround = false;
    }

    public virtual void updateEntity()
    {
        
    }


    public virtual Transform GetCameraHolder()
    {
        return this.transform;
    }

    public virtual Vector3 getLookVector()
    {
        return this.transform.forward;
    }

    public virtual void ApplyInputAction(int id, EntityInputValue input)
    {

    }

    //public virtual void OnRedirectedCollisionEnter(int ID, Collision collision, CollisionRedirect redirectComponent)
    //{

    //}

    //public virtual void OnRedirectedCollisionStay(int ID, Collision collision, CollisionRedirect redirectComponent)
    //{

    //}

    //public virtual void OnRedirectedTriggerEnter(int ID, Collider other, CollisionRedirect redirectComponent)
    //{
    //    if (ID == 0)
    //        onGround = true;
    //}

    //public virtual void OnRedirectedTriggerStay(int ID, Collider other, CollisionRedirect redirectComponent)
    //{
    //    if (ID == 0)
    //        onGround = true;
    //}

    public virtual void DestroyAllColliders()
    {
        MyUtils.DestroyAllColliders(this.gameObject);
    }

}

public class EntityInputValue
{
    public InputValue inputValue;
    public int data;
    public EntityInputValue(InputValue inputValue)
    {
        this.inputValue = inputValue;
    }
    public EntityInputValue(int data)
    {
        this.data = data;
    }
}
