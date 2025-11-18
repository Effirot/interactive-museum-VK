
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor.Playables;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class EntityPlayer : EntityLiving
{
    [HideInInspector]
    public Vector2 playerControlMoving = Vector2.zero;
    public Transform cameraHolder;
    float cameraX = 0;
    float cameraZ = 0;
    public float playerSpeed;

    //public PhysicsMaterial nofriction;
    //public PhysicsMaterial yesfriction;

    public CapsuleCollider capsuleCollider;

    public PickableObject righthandObject;
    public PickableObject lefthandObject;

    public Transform rightHand;
    public Transform leftHand;

    Vector3 rightPickedLastPos;
    Vector3 leftPickedLastPos;
    float rightpickSmoothProgress;
    float leftpickSmoothProgress;

    public RaycastHit lastLookRaycast;
    public Light spotlight;

    public override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        if (righthandObject != null)
        {
            Vector3 posTo = Vector3.Lerp(rightPickedLastPos, rightHand.position, rightpickSmoothProgress);
            righthandObject.transform.position = posTo;
            righthandObject.transform.rotation = rightHand.rotation;
            MyUtils.SetMove(righthandObject.gameObject, 0, 0, 0);

            if (rightpickSmoothProgress < 1)
            {
                rightpickSmoothProgress += 0.05F;
            }
        }
        if (lefthandObject != null)
        {
            Vector3 posTo = Vector3.Lerp(leftPickedLastPos, leftHand.position, leftpickSmoothProgress);
            lefthandObject.transform.position = posTo;
            lefthandObject.transform.rotation = leftHand.rotation;
            MyUtils.SetMove(lefthandObject.gameObject, 0, 0, 0);

            if (leftpickSmoothProgress < 1)
            {
                leftpickSmoothProgress += 0.05F;
            }
        }

    }

    public override void updateEntity()
    {
        float allMult = 8F;
        //float zmax = 3;
        //float zadd = 0.3F;

        if (userInterface != null)
        {
            //MOVE
            playerControlMoving = userInterface.move;



        }



        //if (playerControlMoving.x < 0)
        //{
        //    if (cameraZ < zmax)
        //        cameraZ += zadd;
        //}
        //else
        //if (playerControlMoving.x > 0)
        //{
        //    if (cameraZ > -zmax)
        //        cameraZ -= zadd;
        //}
        //else
        //{
        //    if (cameraZ < 0)
        //        cameraZ += zadd;
        //    if (cameraZ > 0)
        //        cameraZ -= zadd;

        //}
        cameraHolder.eulerAngles = new Vector3(cameraX, this.transform.eulerAngles.y, cameraZ);

        float velX = entityRigidbody.linearVelocity.x;
        float velY = entityRigidbody.linearVelocity.y;
        float velZ = entityRigidbody.linearVelocity.z;


        Vector2 rotated = MyUtils.rotateVector(playerControlMoving, -this.transform.eulerAngles.y);

        Vector3 speedVec = new Vector3(entityRigidbody.linearVelocity.x, 0, entityRigidbody.linearVelocity.z);
        Vector3 direction = new Vector3(rotated.x, 0, rotated.y);

        Vector3 speedInDirection = Vector3.Project(speedVec, direction);
        float magnitude = speedInDirection.magnitude;

        if (Vector3.Angle(speedVec, direction) > 90)
        {
            magnitude = -magnitude;
        }


        //float newlayerMotion2D = (float)Math.Sqrt(newvelX * newvelX + newvelZ * newvelZ);
        bool movingPressed = playerControlMoving.magnitude != 0;
        //if (movingPressed)
        //{
        //    capsuleCollider.material = nofriction;
        //}
        //else
        //{
        //    capsuleCollider.material = yesfriction;
        //}

        if (movingPressed && magnitude < 30)
        {
            float speed = playerSpeed * allMult;
            float friction = 0.85F;//0.98 ^ 5
            float newvelX = velX * friction + rotated.x * speed;
            float newvelZ = velZ * friction + rotated.y * speed;
            velX = newvelX;
            velZ = newvelZ;
        }
        else
        {
            float AIRfriction = 0.9F;
            velX *= AIRfriction;
            velZ *= AIRfriction;
        }

        velY -= 0.07F * allMult;

        

        if (this.transform.position.y < -100)
        {
            this.transform.position = new Vector3(0, 3.54F, 0);
            velY = 0;
        }

        entityRigidbody.linearVelocity = new Vector3(velX, velY, velZ);


        //LOOK RAYCAST
        {
            Ray ray = new Ray(GetCameraHolder().position, GetCameraHolder().forward);
            RaycastHit hit;
            int mask = (1 << 6) | (1 << 0);
            Physics.Raycast(ray, out hit, 4, mask);
            lastLookRaycast = hit;
            if (lastLookRaycast.collider != null)
            {
                Interactable interactable = lastLookRaycast.collider.gameObject.GetComponent<Interactable>();
                if (interactable != null)
                {
                    this.userInterface.canvasManager.setInteractionInfo(interactable.onLookText);
                }
            }
        }


        this.userInterface.updateOnEntity();

    }

    public override void ApplyInputAction(int id, EntityInputValue input)
    {
        base.ApplyInputAction(id, input);

        if (id == 0)
        {
            onClickLook(true, false);
        }
        if (id == 1)
        {
            onClickLook(false, false);
        }
        if (id == 2)
        {
            float mouseSense = userInterface.mouseSense;
            //LOOK
            Vector2 vec = input.inputValue.Get<Vector2>();
            this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y + vec.x * mouseSense, this.transform.eulerAngles.z);
            cameraX = Math.Clamp(cameraX - vec.y * mouseSense, -85F, 85F);
        }
        if (id == 3)
        {
            onClickLook(false, true);
        }
        if (id == 4)
        {
            spotlight.enabled = !spotlight.enabled;
        }



    }

    public void onClickLook(bool isRighthand, bool interact)
    {
        //Debug.Log(isRighthand);
        //Ray ray = new Ray(GetCameraHolder().position, GetCameraHolder().forward);
        //RaycastHit hit;
        //int mask = (1 << 6) | (1 << 0);

        if (lastLookRaycast.collider != null)
        {
            
            GameObject gameObject = lastLookRaycast.collider.gameObject;
            if (interact)
            {
                Interactable iObject = gameObject.GetComponent<Interactable>();
                if (iObject != null)
                {
                    iObject.interact(this, lastLookRaycast.point);
                }
                return;
            }


            bool continuePlaceObject = true;
            PickableObject pickableObject = gameObject.GetComponent<PickableObject>();
            if (pickableObject != null)
            {
                if (pickupObjectToHand(pickableObject, isRighthand))
                    continuePlaceObject = false;
            }

            if (continuePlaceObject)
            {
                placeObjectFromHand(isRighthand, lastLookRaycast);
            }
        }
        else
        {
            dropObjectFromHand(isRighthand);
        }
    }
    public bool pickupObjectToHand(PickableObject pickableObject, bool isRighthand)
    {
        if (isRighthand && righthandObject == null)
        {
            righthandObject = pickableObject;
            pickableObject.onPick();
            rightPickedLastPos = pickableObject.transform.position;
            rightpickSmoothProgress = 0;
            return true;
        }
        if (!isRighthand && lefthandObject == null)
        {
            lefthandObject = pickableObject;
            pickableObject.onPick();
            leftPickedLastPos = pickableObject.transform.position;
            leftpickSmoothProgress = 0;
            return true;
        }
        return false;
    }

    public void placeObjectFromHand(bool isRighthand, RaycastHit hit)
    {
        if (isRighthand && righthandObject != null)
        {

            righthandObject.onPlace(hit.point);
            righthandObject = null;
        }
        if (!isRighthand && lefthandObject != null)
        {
            lefthandObject.onPlace(hit.point);
            lefthandObject = null;
        }
    }
    public void dropObjectFromHand(bool isRighthand)
    {

        if (isRighthand && righthandObject != null)
        {
            MyUtils.AddMove(righthandObject.gameObject, GetCameraHolder().forward * 3);
            righthandObject.onDrop();
            righthandObject = null;
        }
        if (!isRighthand && lefthandObject != null)
        {
            MyUtils.AddMove(lefthandObject.gameObject, GetCameraHolder().forward * 3);
            lefthandObject.onDrop();
            lefthandObject = null;
        }
    }




    public override Vector3 getLookVector()
    {
        Vector3 safe = this.transform.eulerAngles;
        this.transform.eulerAngles = new Vector3(cameraX, this.transform.eulerAngles.y, 0);
        Vector3 look = this.transform.forward;
        this.transform.eulerAngles = safe;
        return look;
    }


    public override Transform GetCameraHolder()
    {
        return cameraHolder;
    }


}


