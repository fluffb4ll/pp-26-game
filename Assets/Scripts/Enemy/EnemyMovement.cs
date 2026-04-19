using Managers;
using UnityEngine;

namespace Enemy
{
    public class EnemyMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 1f;
        [SerializeField] private float stoppingDistance = 1.4f;
        [SerializeField] private float rotationSpeed = 360f;
    
        private GameManager _gameManager;
        private Transform _playerTransform;
        void Awake()
        {
            _gameManager = GameManager.Instance;
            _playerTransform = _gameManager.playerTransform;
        }

        // Update is called once per frame
        void Update()
        {
            HandleMovement();
        }
    
        /// <summary>
        /// Обрабатывает движение врага к игроку
        /// </summary>
        private void HandleMovement()
        {
            if (_gameManager.currentState != GameState.Combat)
                return;

            var toPlayer = _playerTransform.position - transform.position;
            toPlayer.y = 0f;

            var sqrDistance = toPlayer.sqrMagnitude;
            var sqrStoppingDistance = stoppingDistance * stoppingDistance;

            if (sqrDistance <= sqrStoppingDistance || sqrDistance <= 0.0001f)
                return;

            var distance = Mathf.Sqrt(sqrDistance);
            var direction = toPlayer / distance;
            var step = Mathf.Min(moveSpeed * Time.deltaTime, distance - stoppingDistance);

            transform.position += direction * step;

            var targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime);
        }
    }
}
