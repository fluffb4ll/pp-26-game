using Interfaces;
using Managers;
using UnityEngine;

namespace Enemy
{
    public class DefaultEnemyController : MonoBehaviour, IDamageable
    {
        [SerializeField] private GameObject spawnableBrainrot;
        [SerializeField] private float timeBeforeDestroyingBody;
        [SerializeField] private float deathAnimationSpeed;
        
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 1f;
        [SerializeField] private float stoppingDistance = 1.4f;
        [SerializeField] private float rotationSpeed = 360f;
    
        [Header("Health and Damage")]
        public int maxHealth;
        public int currentHealth;
        public int damage;
        
        private bool _isDying;
        private GameManager _gameManager;
        private Transform _playerTransform;
        public SpawnManager spawnManager;
        
        void Awake()
        {
            _gameManager = GameManager.Instance;
            _playerTransform = _gameManager.playerTransform;
        }
        
        void Update()
        {
            if (_isDying)
            {
                if (transform.rotation.x > -0.5f)
                    PlayDeathAnimation();
                else if (timeBeforeDestroyingBody > 0)
                    timeBeforeDestroyingBody -= Time.deltaTime;
                else
                    Destroy(gameObject);
            }
            HandleMovement();
        }
        
        /// <inheritdoc/>
        public void TakeDamage(int damageAmount)
        {
            currentHealth -= damageAmount;
            
            if (currentHealth <= 0)
                Die();
        }

        /// <inheritdoc/>
        public void Heal(int healAmount)
        {
            currentHealth += healAmount;
            
            if (currentHealth > maxHealth)
                currentHealth = maxHealth;
        }
    
        /// <inheritdoc/>
        public void Die()
        {
            currentHealth = 0;
            _isDying = true;
            spawnManager.UnregisterEnemy(gameObject);
            SpawnBrainrot();
        }
    
        /// <summary>
        /// Проигрывает анимацию смерти - переворачивает врага на "спину"
        /// </summary>
        private void PlayDeathAnimation()
        {
            Quaternion currentRot = transform.rotation;
            Quaternion targetRot = currentRot * Quaternion.AngleAxis(90f, new Vector3(-90f, currentRot.y, 0f));
            transform.rotation = 
                Quaternion.RotateTowards(
                    currentRot,
                    targetRot, 
                    Time.deltaTime * deathAnimationSpeed);
        }

        /// <summary>
        /// Спавнит брейнрота после смерти персонажа
        /// </summary>
        private void SpawnBrainrot()
        {
            Instantiate(spawnableBrainrot, transform.position, Quaternion.identity);
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
