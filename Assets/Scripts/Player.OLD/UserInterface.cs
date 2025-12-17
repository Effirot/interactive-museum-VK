
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class UserInterface : MonoBehaviour
{


    public EntityLiving entityUnderControl = null;

    [HideInInspector]
    public Vector2 move;
    [HideInInspector]
    public Vector2 look;
    [HideInInspector]
    public bool jump;
    [HideInInspector]
    public bool leftMouseButton;
    [HideInInspector]
    public bool rightMouseButton;
    [HideInInspector]
    public bool interact;
    [HideInInspector]
    public bool spotlight;

    public CanvasManager canvasManager;

    public static Vector3 positionCamera;
    public float mouseSense = 0.06F;

    public void ApplyInputToEntity(int id, InputValue inputValue)
    {
        if (entityUnderControl != null)
        {
            entityUnderControl.ApplyInputAction(id, new EntityInputValue(inputValue));
        }
    }
    public void ApplyInputToEntity(int id, int inputValue)
    {
        if (entityUnderControl != null)
        {
            entityUnderControl.ApplyInputAction(id, new EntityInputValue(inputValue));
        }
    }

    public void OnMove(InputValue inputValue) 
    { 
        //ApplyInputToEntity(1, inputValue); 
        move = inputValue.Get<Vector2>();
    }
    public void OnLook(InputValue inputValue) 
    { 
        ApplyInputToEntity(2, inputValue);
        
    }
    public void OnJump(InputValue inputValue) { jump = inputValue.isPressed; }
    public void OnLeftMouseButton(InputValue inputValue) 
    { 
        if (!leftMouseButton && inputValue.isPressed)
            ApplyInputToEntity(0, inputValue);
        leftMouseButton = inputValue.isPressed;

        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OnRightMouseButton(InputValue inputValue)
    {
        if (!rightMouseButton && inputValue.isPressed)
            ApplyInputToEntity(1, inputValue);
        rightMouseButton = inputValue.isPressed;

    }

    public void OnInteract(InputValue inputValue)
    {
        if (!interact && inputValue.isPressed)
            ApplyInputToEntity(3, inputValue);
        interact = inputValue.isPressed;
    }

    public void OnTurnLight(InputValue inputValue)
    {
        if (!spotlight && inputValue.isPressed)
            ApplyInputToEntity(4, inputValue);
        spotlight = inputValue.isPressed;
    }


    // Start is called before the first frame update
    void Start()
    {
        if (entityUnderControl != null)
            controlEntity(entityUnderControl);

        //GameObject canvasObject = GameObject.Find("Canvas");
        //if (canvasObject != null)
        //{
        //    canvas = canvasObject.GetComponent<Canvas>();
            
        //}

    }

    public void Update()
    {
        if (entityUnderControl != null)
            positionCamera = entityUnderControl.GetCameraHolder().position;
    }

    // Update is called once per frame
    public void updateOnEntity()
    {
        
        //jump = false;
        //dash = false;
        //leftMouseButton = false;
        //rightMouseButton = false;
        //grab = false;

    }

    public void controlEntity(EntityLiving entity)
    {
        if (entityUnderControl != null)
            this.entityUnderControl.userInterface = null;
        this.entityUnderControl = entity;
        this.entityUnderControl.userInterface = this;
    }


     


}

