using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    /// <summary>
    /// двигает игрока по xz относительно камеры
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private InputActionReference moveActionReference;
        [SerializeField] private InputActionReference jumpActionReference;
        [SerializeField] private Transform cameraTransform;

        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float turnSpeed = 720f;
        [SerializeField] private float jumpStrength = 6f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float groundedVerticalVelocity = -2f;

        private CharacterController _characterController;
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private float _verticalVelocity;
        private Transform _transform;

        /// <summary>
        /// забираем ссылки один раз на старте
        /// </summary>
        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _moveAction = moveActionReference.action;
            _jumpAction = jumpActionReference.action;
            _transform = transform;
        }

        /// <summary>
        /// включаем move когда игрок активен
        /// </summary>
        private void OnEnable()
        {
            _moveAction.Enable();
            _jumpAction.Enable();
        }

        /// <summary>
        /// выключаем move вместе с игроком
        /// </summary>
        private void OnDisable()
        {
            _moveAction.Disable();
            _jumpAction.Disable();
        }

        /// <summary>
        /// обновляем движение каждый кадр
        /// </summary>
        private void Update()
        {
            HandleMovement();
        }

        /// <summary>
        /// считаем движение и поворот игрока
        /// </summary>
        private void HandleMovement()
        {
            bool isGrounded = _characterController.isGrounded;

            if (isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = groundedVerticalVelocity;

            Vector2 input = _moveAction.ReadValue<Vector2>();
            Vector3 movement = Vector3.zero;

            if (input.sqrMagnitude > 0.01f)
            {
                // вычисление направления движения относительно поворота камеры
                Vector3 camForward = cameraTransform.forward;
                Vector3 camRight = cameraTransform.right;

                camForward.y = 0f;
                camRight.y = 0f;
                camForward = camForward.normalized;
                camRight = camRight.normalized;

                movement = camForward * input.y + camRight * input.x;

                if (movement.sqrMagnitude > 1f)
                    movement.Normalize();

                Quaternion targetRotation = Quaternion.LookRotation(movement, Vector3.up);
                _transform.rotation = Quaternion.RotateTowards(
                    _transform.rotation,
                    targetRotation,
                    turnSpeed * Time.deltaTime);
            }

            if (isGrounded && _jumpAction.WasPressedThisFrame())
                _verticalVelocity = jumpStrength;

            _verticalVelocity += gravity * Time.deltaTime;

            Vector3 frameMotion = movement * moveSpeed;
            frameMotion.y = _verticalVelocity;

            _characterController.Move(frameMotion * Time.deltaTime);
        }
    }
}
