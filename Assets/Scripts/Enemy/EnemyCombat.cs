using Interfaces;
using Managers;
using Player;
using UnityEngine;

namespace Enemy
{
    public class EnemyCombat : MonoBehaviour, IDamageable
    {
        [SerializeField] private GameObject spawnableBrainrot;
        [SerializeField] private float timeBeforeDestroyingBody;
        [SerializeField] private float deathAnimationSpeed;
        
        [SerializeField] private int maxHealth;
        [SerializeField] private int currentHealth;
        [SerializeField] private int damage;
        [SerializeField] private float attackRate;
        [SerializeField] private float attackDistance;
        
        public SpawnManager spawnManager;
        private GameManager _gameManager;
        private Transform _playerTransform;
        private bool _isDying;
        private float _attackTimer;
        private PlayerController _playerController;
        
        void Awake()
        {
            currentHealth = maxHealth;
            _gameManager = GameManager.Instance;
            _playerTransform = _gameManager.playerTransform;
            _playerController = _gameManager.playerController;
            _attackTimer = attackRate;
        }
        
        void Update()
        {
            HandleDeath();
            HandleDamageByProximity();
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
            
            if (spawnManager is not null)
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
        /// Проигрывает анимацию смерти, затем, спустя некоторое время, уничтожает врага
        /// </summary>
        private void HandleDeath()
        {
            if (!_isDying)
                return;
            
            if (transform.rotation.x > -0.5f)
                PlayDeathAnimation();
            else if (timeBeforeDestroyingBody > 0)
                timeBeforeDestroyingBody -= Time.deltaTime;
            else
                Destroy(gameObject);
        }

        /// <summary>
        /// Пробует нанести урон по игроку при приближении к нему
        /// </summary>
        private void HandleDamageByProximity()
        {
            if (_gameManager.currentState != GameState.Combat)
                return;
            
            if (_attackTimer > 0f)
                _attackTimer -= Time.deltaTime;

            var toPlayer = _playerTransform.position - transform.position;
            toPlayer.y = 0f;

            if (toPlayer.sqrMagnitude > attackDistance * attackDistance || _attackTimer > 0f)
                return;

            _playerController.TakeDamage(damage);
            _attackTimer = attackRate;
        }
    }
}
