using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private InputActionReference moveActionReference;
        [SerializeField] private Transform cameraTransform;

        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float turnSpeed = 90f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float groundedVerticalVelocity = -2f;

        private CharacterController _characterController;
        private float _verticalVelocity;
        private Transform _transform;
        
        /// <summary>
        /// Обрабатывает движение игрока
        /// </summary>
        private void HandleMovement()
        {
            if (_characterController.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = groundedVerticalVelocity;

            Vector2 input = moveActionReference.action.ReadValue<Vector2>();
            Vector3 movement = Vector3.zero;
            
            if (input.sqrMagnitude > 0.01f)
            {
                // вычисление направления поворота относительно камеры
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
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    turnSpeed * Time.deltaTime);
            }

            _verticalVelocity += gravity * Time.deltaTime;

            Vector3 frameMotion = movement * moveSpeed;
            frameMotion.y = _verticalVelocity;
            
            if (frameMotion.sqrMagnitude > 1f) 
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