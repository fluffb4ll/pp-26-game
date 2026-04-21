using Managers;
using UnityEngine;

namespace Enemy
{
    /// <summary>
    /// двигает врага к игроку через character controller
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public class EnemyMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 1f;
        [SerializeField] private float stoppingDistance = 1.4f;
        [SerializeField] private float rotationSpeed = 360f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float groundedVerticalVelocity = -2f;

        private CharacterController _characterController;
        private GameManager _gameManager;
        private Transform _playerTransform;
        private Transform _transform;
        private float _verticalVelocity;

        /// <summary>
        /// кешируем ссылки один раз
        /// </summary>
        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _gameManager = GameManager.Instance;
            _playerTransform = _gameManager.playerTransform;
            _transform = transform;
        }

        /// <summary>
        /// обновляем движение каждый кадр
        /// </summary>
        private void Update()
        {
            HandleMovement();
        }

        /// <summary>
        /// двигает врага к игроку с учётом стен и земли
        /// </summary>
        private void HandleMovement()
        {
            if (_characterController.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = groundedVerticalVelocity;

            var frameMotion = Vector3.zero;

            if (_gameManager.currentState == GameState.Combat)
                frameMotion = GetHorizontalMotion();

            _verticalVelocity += gravity * Time.deltaTime;
            frameMotion.y = _verticalVelocity * Time.deltaTime;

            _characterController.Move(frameMotion);
        }

        /// <summary>
        /// считает движение по xz без прохода за дистанцию остановки
        /// </summary>
        private Vector3 GetHorizontalMotion()
        {
            var toPlayer = _playerTransform.position - _transform.position;
            toPlayer.y = 0f;

            var sqrDistance = toPlayer.sqrMagnitude;
            var sqrStoppingDistance = stoppingDistance * stoppingDistance;

            if (sqrDistance <= sqrStoppingDistance || sqrDistance <= 0.0001f)
                return Vector3.zero;

            var distance = Mathf.Sqrt(sqrDistance);
            var direction = toPlayer / distance;
            var step = Mathf.Min(moveSpeed * Time.deltaTime, distance - stoppingDistance);

            var targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            _transform.rotation = Quaternion.RotateTowards(
                _transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime);

            return direction * step;
        }
    }
}
