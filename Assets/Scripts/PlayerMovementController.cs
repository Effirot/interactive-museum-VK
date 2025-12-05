using System.Runtime.CompilerServices;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovementController : MonoBehaviour
{

    public static PlayerMovementController Current { get; private set; } = null;

    [SerializeField]
    private CinemachineCamera headCamera;

    [Space]
    [SerializeField, Range(0, 50)]
    private float interactionDistance = 5;
    [SerializeField]
    private LayerMask interactionLayer = 1 << 0;

    [Space]
    [SerializeField, Range(0, 50)]
    private float speed = 10;
    [SerializeField, Range(0, 50)]
    private float sprintSpeed = 10;
    [SerializeField, Range(0, 2)]
    private float characterSense = 0.2f;


    public bool IsGrounded { get; private set; } = false;

    private CharacterController characterController;
    private PlayerInput input;
    
    private Vector3 movementDirection = Vector3.zero;
    private Vector2 lookDirection = Vector2.zero;

    private Vector3 localMovementAcelerationVector = Vector3.zero;

    private bool SprintState = false;
    
    private Vector3 velocity = Vector3.zero;
    private float contollerHitResetTimeout = 0;

    private Vector3 resultmovementDirection = Vector3.zero;

    public void StartControlling()
    {
        input.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void StopControlling()
    {
        input.enabled = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput>();
    
        StartControlling();
        Current = this;
    }
    private void FixedUpdate()
    {
        ResetCollisionData();
        CalculateVelocity(ref velocity);

        resultmovementDirection = velocity * 5f + CalculateMovementDirection();
    }
    private void LateUpdate()
    {
        var timescale = Time.deltaTime * 20f;

        transform.rotation = 
            Quaternion.Lerp(
                transform.rotation, 
                Quaternion.Euler(0, lookDirection.x, 0), 
                timescale);
        
        headCamera.transform.localRotation = 
            Quaternion.Lerp(
                headCamera.transform.localRotation, 
                Quaternion.Euler(-lookDirection.y, 0, 0), 
                timescale);

        characterController.Move(resultmovementDirection * Time.deltaTime);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (headCamera == null)
        {
            headCamera = GetComponentInChildren<CinemachineCamera>();
        }
    }
#endif

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        contollerHitResetTimeout = 0.1f;

        IsGrounded = Vector3.Angle(hit.normal, Vector3.up) <= 35;


        var normalAngle = Quaternion.FromToRotation(hit.normal, Vector3.down);

        var deltaVelocity = normalAngle * velocity;
        deltaVelocity.y = Mathf.Min(0, deltaVelocity.y);


        if (IsGrounded)
        {
            deltaVelocity.x = 0;    
            deltaVelocity.z = 0;
        }

        velocity = Quaternion.Inverse(normalAngle) * deltaVelocity;
    }

    private void OnMove(InputValue inputValue)
    {
        var input = inputValue.Get<Vector2>();
        movementDirection = new Vector3(input.x, 0, input.y); 
    } 
    private void OnLook(InputValue inputValue)
    {
        lookDirection += inputValue.Get<Vector2>() * characterSense;

        lookDirection.y = Mathf.Clamp(lookDirection.y, -89, 89);
    }
    private void OnJump(InputValue inputValue)
    {
        if (inputValue.isPressed && IsGrounded)
        {
            velocity = Vector3.up * 3;
        }
    }
    private void OnSprint(InputValue inputValue)
    {
        SprintState = inputValue.isPressed;
    }
    private void OnInteract(InputValue inputValue)
    {
        if (headCamera.IsLive &&
            Physics.Raycast(
                headCamera.transform.position, 
                headCamera.transform.forward, 
                out var hit, 
                interactionDistance, 
                interactionLayer))
        {
            hit.collider.SendMessage("Interact", gameObject, SendMessageOptions.DontRequireReceiver);
        }
    }

    private Vector3 CalculateMovementDirection()
    {
        if (headCamera.IsLive)
        {
            localMovementAcelerationVector = Vector3.Lerp(localMovementAcelerationVector, transform.rotation * movementDirection * (SprintState ? sprintSpeed : speed), (IsGrounded ? 10 : 1) * Time.fixedDeltaTime);
        }
        else
        {
            localMovementAcelerationVector = Vector3.zero;
        }

        return localMovementAcelerationVector; 
    }
    private void CalculateVelocity(ref Vector3 velocity)
    {
        velocity = Vector3.Lerp(velocity, Physics.gravity, Time.fixedDeltaTime);
    }

    private void ResetCollisionData()
    {
        contollerHitResetTimeout -= Time.fixedDeltaTime;
        
        if (contollerHitResetTimeout < 0)
        {
            IsGrounded = false;
        }
    }
}
