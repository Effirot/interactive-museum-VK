
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

    public Canvas canvas;


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
    public void OnLeftMouseButton(InputValue inputValue) { leftMouseButton = inputValue.isPressed; }

    public void OnRightMouseButton(InputValue inputValue) { rightMouseButton = inputValue.isPressed; }


    // Start is called before the first frame update
    void Start()
    {
        if (entityUnderControl != null)
            controlEntity(entityUnderControl);

        GameObject canvasObject = GameObject.Find("Canvas");
        if (canvasObject != null)
        {
            canvas = canvasObject.GetComponent<Canvas>();
            
        }

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

