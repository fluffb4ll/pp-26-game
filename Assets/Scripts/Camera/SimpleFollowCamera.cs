using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public sealed class SimpleFollowCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string actionMapName = "Player";
    [SerializeField] private string lookActionName = "Look";
    [SerializeField] private float pivotHeight = 1.6f;
    [SerializeField] private float distance = 6f;
    [SerializeField] private float followSmoothTime = 0.06f;
    [SerializeField] private float minPitch = -10f;
    [SerializeField] private float maxPitch = 70f;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float touchSensitivity = 0.08f;
    [SerializeField] private float gamepadLookSpeed = 140f;
    [SerializeField] private bool lockCursorWhileRotating = true;

    private InputAction lookAction;
    private Transform cachedTransform;
    private Vector3 followVelocity;
    private bool cursorLockedByCamera;
    private float yaw;
    private float pitch;

    private void Awake()
    {
        cachedTransform = transform;
        lookAction = inputActions != null
            ? inputActions.FindAction($"{actionMapName}/{lookActionName}", false)
            : null;

        if (lookAction == null)
        {
            Debug.LogError("SimpleFollowCamera could not find the configured Look action.", this);
            enabled = false;
            return;
        }

        Vector3 eulerAngles = cachedTransform.eulerAngles;
        yaw = NormalizeAngle(eulerAngles.y);
        pitch = Mathf.Clamp(NormalizeAngle(eulerAngles.x), minPitch, maxPitch);
    }

    private void OnEnable()
    {
        lookAction?.Enable();
    }

    private void OnDisable()
    {
        lookAction?.Disable();
        ReleaseCursor();
    }

    private void OnDestroy()
    {
        ReleaseCursor();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            ReleaseCursor();
        }
    }

    private void LateUpdate()
    {
        if (target == null || lookAction == null)
        {
            return;
        }

        Vector2 lookInput = ReadLookInput();
        yaw += lookInput.x;
        pitch = Mathf.Clamp(pitch - lookInput.y, minPitch, maxPitch);

        Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 pivotPosition = target.position + Vector3.up * pivotHeight;
        Vector3 desiredPosition = pivotPosition + orbitRotation * (Vector3.back * distance);

        if (followSmoothTime > 0f)
        {
            cachedTransform.position = Vector3.SmoothDamp(
                cachedTransform.position,
                desiredPosition,
                ref followVelocity,
                followSmoothTime);
        }
        else
        {
            cachedTransform.position = desiredPosition;
        }

        cachedTransform.rotation = orbitRotation;
    }

    private Vector2 ReadLookInput()
    {
        UpdateCursorState();

        if (TryReadTouchLook(out Vector2 touchLook))
        {
            return touchLook * touchSensitivity;
        }

        Vector2 rawInput = lookAction.ReadValue<Vector2>();
        InputDevice activeDevice = lookAction.activeControl?.device;

        if (activeDevice is Gamepad)
        {
            return rawInput * (gamepadLookSpeed * Time.deltaTime);
        }

        if (activeDevice is Mouse && Mouse.current != null && Mouse.current.rightButton.isPressed)
        {
            return rawInput * mouseSensitivity;
        }

        return Vector2.zero;
    }

    private void UpdateCursorState()
    {
        if (!lockCursorWhileRotating || Mouse.current == null)
        {
            ReleaseCursor();
            return;
        }

        if (Mouse.current.rightButton.isPressed)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            cursorLockedByCamera = true;
            return;
        }

        ReleaseCursor();
    }

    private void ReleaseCursor()
    {
        if (!cursorLockedByCamera)
        {
            return;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        cursorLockedByCamera = false;
    }

    private static bool TryReadTouchLook(out Vector2 touchLook)
    {
        touchLook = Vector2.zero;

        if (Touchscreen.current == null)
        {
            return false;
        }

        foreach (var touch in Touchscreen.current.touches)
        {
            if (!touch.press.isPressed)
            {
                continue;
            }

            if (touch.position.ReadValue().x < Screen.width * 0.5f)
            {
                continue;
            }

            Vector2 delta = touch.delta.ReadValue();
            if (delta.sqrMagnitude <= 0f)
            {
                continue;
            }

            touchLook = delta;
            return true;
        }

        return false;
    }

    private static float NormalizeAngle(float angle)
    {
        while (angle > 180f)
        {
            angle -= 360f;
        }

        while (angle < -180f)
        {
            angle += 360f;
        }

        return angle;
    }
}
