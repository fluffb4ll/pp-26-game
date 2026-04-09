using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public sealed class PlayerMovement : MonoBehaviour
{
    [SerializeField] private InputActionReference moveActionReference;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float turnSpeed = 540f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedVerticalVelocity = -2f;

    private CharacterController characterController;
    private Transform cachedTransform;
    private Transform cachedVisualRoot;
    private float verticalVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        cachedTransform = transform;
        cachedVisualRoot = visualRoot != null ? visualRoot : cachedTransform;

        if (moveActionReference == null || moveActionReference.action == null)
        {
            Debug.LogError("Move Action Reference is missing.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        moveActionReference?.action?.Enable();
    }

    private void OnDisable()
    {
        moveActionReference?.action?.Disable();
    }

    private void Update()
    {
        if (moveActionReference == null || moveActionReference.action == null)
        {
            return;
        }

        Transform movementCamera = cameraTransform != null ? cameraTransform : Camera.main?.transform;
        Vector2 input = moveActionReference.action.ReadValue<Vector2>();
        Vector3 movement = GetCameraRelativeMovement(input, movementCamera);

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundedVerticalVelocity;
        }

        if (movement.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement, Vector3.up);
            cachedVisualRoot.rotation = Quaternion.RotateTowards(
                cachedVisualRoot.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime);
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 frameMotion = movement * moveSpeed;
        frameMotion.y = verticalVelocity;

        characterController.Move(frameMotion * Time.deltaTime);
    }

    private static Vector3 GetCameraRelativeMovement(Vector2 input, Transform movementCamera)
    {
        if (input.sqrMagnitude <= 0f)
        {
            return Vector3.zero;
        }

        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        if (movementCamera != null)
        {
            forward = movementCamera.forward;
            right = movementCamera.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
        }

        Vector3 movement = forward * input.y + right * input.x;
        if (movement.sqrMagnitude > 1f)
        {
            movement.Normalize();
        }

        return movement;
    }
}
