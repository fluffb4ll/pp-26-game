using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private InputActionReference moveActionReference;

        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float turnSpeed = 720f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float groundedVerticalVelocity = -2f;

        private CharacterController _characterController;
        private float _verticalVelocity;
        
        /// <summary>
        /// Обрабатывает инпуты движения игрока
        /// </summary>
        private void HandleMovement()
        {
            if (_characterController.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = groundedVerticalVelocity;

            Vector2 input = moveActionReference.action.ReadValue<Vector2>();
            Vector3 movement = new Vector3(input.x, 0f, input.y);
            
            if (movement.sqrMagnitude > 0.01f)
            {
                if (movement.sqrMagnitude > 1f)
                    movement.Normalize();
            
                Quaternion targetRotation = Quaternion.LookRotation(movement, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    turnSpeed * Time.deltaTime);
            }

            _verticalVelocity += gravity * Time.deltaTime;

            Vector3 frameMotion = movement * moveSpeed;
            frameMotion.y = _verticalVelocity;

            _characterController.Move(frameMotion * Time.deltaTime);
        }
        
        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
        
            if (moveActionReference == null)
            {
                Debug.LogError("Move Action Reference is missing.", this);
                enabled = false;
            }
        }

        private void OnEnable() => moveActionReference?.action.Enable();

        private void OnDisable() => moveActionReference?.action.Disable();

        private void Update()
        {
            HandleMovement();
        }
    }
}
