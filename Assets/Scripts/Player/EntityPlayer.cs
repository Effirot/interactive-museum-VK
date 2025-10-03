
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor.Playables;
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

    public PhysicsMaterial nofriction;
    public PhysicsMaterial yesfriction;

    public CapsuleCollider capsuleCollider;

    public override void Start()
    {
        base.Start();
    }

    public override void updateEntity()
    {
        float allMult = 8F;
        float zmax = 3;
        float zadd = 0.3F;

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
        if (movingPressed)
        {
            capsuleCollider.material = nofriction;
        }
        else
        {
            capsuleCollider.material = yesfriction;
        }
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


        this.userInterface.updateOnEntity();

    }

    public override void ApplyInputAction(int id, EntityInputValue input)
    {
        base.ApplyInputAction(id, input);

        if (id == 2)
        {

            //LOOK
            Vector2 vec = input.inputValue.Get<Vector2>();
            this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y + vec.x * 0.25F, this.transform.eulerAngles.z);
            cameraX = Math.Clamp(cameraX - vec.y * 0.25F, -85F, 85F);
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


