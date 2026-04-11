using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// держит камеру рядом с игроком и крутит ее только от ввода
/// </summary>
[DisallowMultipleComponent]
public sealed class SimpleFollowCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private InputActionReference lookActionReference;
    [SerializeField] private float pivotHeight = 1.6f;
    [SerializeField] private float distance = 6f;
    [SerializeField] private float minPitch = -10f;
    [SerializeField] private float maxPitch = 70f;
    [SerializeField] private float mouseSensitivity = 0.18f;
    [SerializeField] private float gamepadLookSpeed = 220f;
    [SerializeField] private bool lockCursorWhileRotating = true;

    private InputAction _lookAction;
    private Transform _transform;
    private bool _cursorLockedByCamera;
    private float _yaw;
    private float _pitch;

    /// <summary>
    /// забираем ссылки и стартовые углы камеры
    /// </summary>
    private void Awake()
    {
        _lookAction = lookActionReference.action;
        _transform = transform;

        Vector3 eulerAngles = _transform.eulerAngles;
        _yaw = NormalizeAngle(eulerAngles.y);
        _pitch = Mathf.Clamp(NormalizeAngle(eulerAngles.x), minPitch, maxPitch);
    }

    /// <summary>
    /// включаем look пока камера активна
    /// </summary>
    private void OnEnable() => _lookAction.Enable();

    /// <summary>
    /// отпускаем ввод который забрала камера
    /// </summary>
    private void OnDisable()
    {
        _lookAction.Disable();
        ReleaseCursor();
    }

    /// <summary>
    /// отпускаем курсор если камеру удалили во время поворота
    /// </summary>
    private void OnDestroy()
    {
        ReleaseCursor();
    }

    /// <summary>
    /// не даем курсору залипнуть после потери фокуса
    /// </summary>
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            ReleaseCursor();
    }

    /// <summary>
    /// ставим камеру после движения игрока
    /// </summary>
    private void LateUpdate()
    {
        Vector2 lookInput = ReadLookInput();
        _yaw += lookInput.x;
        _pitch = Mathf.Clamp(_pitch - lookInput.y, minPitch, maxPitch);

        Quaternion orbitRotation = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 pivotPosition = target.position + Vector3.up * pivotHeight;

        _transform.position = pivotPosition + orbitRotation * (Vector3.back * distance);
        _transform.rotation = orbitRotation;
    }

    /// <summary>
    /// переводим look в поворот камеры
    /// </summary>
    private Vector2 ReadLookInput()
    {
        UpdateCursorState();

        Vector2 rawInput = _lookAction.ReadValue<Vector2>();
        if (rawInput.sqrMagnitude <= 0.0001f)
            return Vector2.zero;

        InputDevice activeDevice = _lookAction.activeControl.device;

        if (activeDevice is Gamepad or Joystick)
            return rawInput * (gamepadLookSpeed * Time.deltaTime);

        if (activeDevice is Mouse && Mouse.current is not null && Mouse.current.rightButton.isPressed)
            return rawInput * mouseSensitivity;

        return Vector2.zero;
    }

    /// <summary>
    /// прячем курсор только пока игрок крутит камеру пкм
    /// </summary>
    private void UpdateCursorState()
    {
        if (!lockCursorWhileRotating || Mouse.current is null)
        {
            ReleaseCursor();
            return;
        }

        if (Mouse.current.rightButton.isPressed)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _cursorLockedByCamera = true;
            return;
        }

        ReleaseCursor();
    }

    /// <summary>
    /// возвращаем курсор если его забрала эта камера
    /// </summary>
    private void ReleaseCursor()
    {
        if (!_cursorLockedByCamera)
            return;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _cursorLockedByCamera = false;
    }

    /// <summary>
    /// приводим угол к нормальному диапазону
    /// </summary>
    private static float NormalizeAngle(float angle)
    {
        while (angle > 180f)
            angle -= 360f;

        while (angle < -180f)
            angle += 360f;

        return angle;
    }
}
