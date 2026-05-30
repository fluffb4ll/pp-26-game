using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Camera
{
    /// <summary>
    /// держит камеру рядом с игроком и крутит её только от ввода
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SimpleFollowCamera : MonoBehaviour
    {
        public static SimpleFollowCamera Instance { get; private set; }
    
        [SerializeField] private Transform target;
        [SerializeField] private InputActionReference lookCamMovementActionReference;
        [SerializeField] private InputActionReference lookLockPtrActionReference;
        [SerializeField] private float pivotHeight = 1.6f;
        [SerializeField] private float distance = 6f;
        [SerializeField] private float minPitch = -10f;
        [SerializeField] private float maxPitch = 70f;
        [SerializeField] private bool lockCursorWhileRotating = true;
        [SerializeField] [Range(0, 100)] private int lookSensitivity = 100;

        [SerializeField] private float xMult;
        [SerializeField] private float yMult;

        private InputAction _lookCamMovementAction;
        private InputAction _lookLockPtrAction;
        private Transform _transform;
        private bool _cursorLockedByCamera;
        private float _yaw;
        private float _pitch;
        
        private Action<Quaternion> _onCamRotation;

        /// <summary>
        /// кешируем actions и стартовые углы камеры
        /// </summary>
        private void Awake()
        {
            if (!ReferenceEquals(Instance, null) && !ReferenceEquals(Instance, this))
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            
            _lookCamMovementAction = lookCamMovementActionReference.action;
            _lookLockPtrAction = lookLockPtrActionReference.action;
            _transform = transform;

            var eulerAngles = _transform.eulerAngles;
            _yaw = NormalizeAngle(eulerAngles.y);
            _pitch = Mathf.Clamp(NormalizeAngle(eulerAngles.x), minPitch, maxPitch);
        }

        /// <summary>
        /// включаем look actions пока камера активна
        /// </summary>
        private void OnEnable()
        {
            _lookCamMovementAction.Enable();
            _lookLockPtrAction.Enable();
        }

        /// <summary>
        /// выключаем look actions и возвращаем курсор
        /// </summary>
        private void OnDisable()
        {
            _lookLockPtrAction.Disable();
            _lookCamMovementAction.Disable();
            ReleaseCursor();
        }

        /// <summary>
        /// возвращаем курсор если камеру удалили во время вращения
        /// </summary>
        private void OnDestroy()
        {
            ReleaseCursor();
        }

        /// <summary>
        /// не даём курсору залипнуть после потери фокуса
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
            HandleLookInput();
        }
        
        public event Action<Quaternion> OnCamRotation
        {
            add => _onCamRotation += value;
            remove => _onCamRotation -= value;
        }

        private void HandleLookInput()
        {
            var lookInput = ReadLookInput();
            _yaw += lookInput.x * Time.deltaTime * lookSensitivity;
            _pitch -= lookInput.y * Time.deltaTime * lookSensitivity;
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

            var orbitRotation = Quaternion.Euler(_pitch, _yaw, 0f);
            var pivotPosition = target.position + Vector3.up * pivotHeight;
            var back = Vector3.back;
            
            _transform.position = pivotPosition + orbitRotation * (new Vector3(back.x + xMult, back.y + yMult, back.z) * distance);
            _transform.rotation = orbitRotation;
            
            _onCamRotation?.Invoke(orbitRotation);
        }

        /// <summary>
        /// переводит input в поворот камеры
        /// </summary>
        private Vector2 ReadLookInput()
        {
            UpdateCursorState();
            var rawInput = _lookCamMovementAction.ReadValue<Vector2>();
            return rawInput.sqrMagnitude <= 0.0001f ? Vector2.zero : rawInput;
        }

        /// <summary>
        /// прячет курсор только пока игрок крутит камеру мышью
        /// </summary>
        private void UpdateCursorState()
        {
            if (!lockCursorWhileRotating || ReferenceEquals(Mouse.current, null))
            {
                ReleaseCursor();
                return;
            }

            if (_lookLockPtrAction.IsPressed())
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                _cursorLockedByCamera = true;
                return;
            }

            ReleaseCursor();
        }

        /// <summary>
        /// возвращает курсор если его забрала эта камера
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
        /// приводит угол к нормальному диапазону
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
}
