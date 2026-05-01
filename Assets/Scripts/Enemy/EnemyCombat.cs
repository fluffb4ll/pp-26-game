using Interfaces;
using Managers;
using Player;
using UnityEngine;

namespace Enemy
{
    /// <summary>
    /// отвечает за здоровье атаку и смерть врага
    /// </summary>
    public class EnemyCombat : MonoBehaviour, IDamageable
    {
        private const int MaxAttackHits = 8;

        private GameObject _spawnableBrainrot;
        [SerializeField] private float timeBeforeDestroyingBody;
        [SerializeField] private float deathAnimationSpeed;

        [SerializeField] private int maxHealth;
        [SerializeField] private int currentHealth;
        [SerializeField] private int damage;
        [SerializeField] private float attackRate;
        [SerializeField] private float attackDistance;
        [SerializeField] private Vector3 attackRayOffset = new (0f, 0.6f, 0f);
        [SerializeField] private Vector3 playerRayOffset = new (0f, 0.9f, 0f);
        [SerializeField] private LayerMask attackRaycastMask = ~0;
        [SerializeField] private EnemyMovement enemyMovement;
        
        public SpawnManager spawnManager;

        private GameManager _gameManager;
        private Transform _playerTransform;
        private Transform _transform;
        private bool _isDying;
        private float _attackTimer;
        private float _destroyTimer;
        private Quaternion _deathTargetRotation;
        private PlayerController _playerController;
        private int _baseMaxHealth;
        private readonly RaycastHit[] _attackHits = new RaycastHit[MaxAttackHits];

        /// <summary>
        /// кешируем нужные ссылки один раз
        /// </summary>
        private void Awake()
        {
            _gameManager = GameManager.Instance;
            _playerTransform = _gameManager.playerTransform;
            _playerController = _gameManager.playerController;
            _transform = transform;
        }

        public void InitializeSpawn(SpawnManager owner, int healthBonus)
        {
            spawnManager = owner;
            maxHealth += healthBonus;
            currentHealth = maxHealth;
            _attackTimer = attackRate;
        }

        /// <summary>
        /// обновляем смерть и попытку атаки
        /// </summary>
        private void Update()
        {
            if (_isDying)
            {
                HandleDeath();
                return;
            }

            HandleDamageByProximity();
        }

        /// <inheritdoc/>
        public void TakeDamage(int damageAmount)
        {
            if (_isDying)
                return;

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
            if (_isDying)
                return;

            currentHealth = 0;
            _isDying = true;
            enemyMovement.setIsDying(true);
            _destroyTimer = timeBeforeDestroyingBody;
            _deathTargetRotation = _transform.rotation * Quaternion.Euler(-90f, 0f, 0f);
            
            spawnManager?.UnregisterEnemy(gameObject);
            _gameManager.ResetCombatSpawnHealthBonus();
            //_gameManager.RegisterEnemyKill();
            
            SpawnBrainrot();
        }

        /// <summary>
        /// проигрывает простую анимацию смерти через поворот
        /// </summary>
        private bool PlayDeathAnimation()
        {
            _transform.rotation = Quaternion.RotateTowards(
                _transform.rotation,
                _deathTargetRotation,
                Time.deltaTime * deathAnimationSpeed);

            return Quaternion.Angle(_transform.rotation, _deathTargetRotation) <= 0.1f;
        }

        /// <summary>
        /// спавнит награду после смерти врага
        /// </summary>
        private void SpawnBrainrot()
        {
            if (_spawnableBrainrot is not null)
                Instantiate(_spawnableBrainrot, _transform.position, Quaternion.identity);
        }

        /// <summary>
        /// проигрывает смерть и потом удаляет тело
        /// </summary>
        private void HandleDeath()
        {
            if (!PlayDeathAnimation())
                return;

            if (_destroyTimer > 0f)
            {
                _destroyTimer -= Time.deltaTime;
                Debug.Log(_destroyTimer);
                return;
            }

            Destroy(gameObject);
        }

        /// <summary>
        /// наносит урон только рядом и только по прямой линии
        /// </summary>
        private void HandleDamageByProximity()
        {
            if (_gameManager.currentState != GameState.Combat)
                return;

            if (_attackTimer > 0f)
            {
                _attackTimer -= Time.deltaTime;
                return;
            }

            var enemyPosition = _transform.position;
            var playerPosition = _playerTransform.position;
            var toPlayer = playerPosition - enemyPosition;
            var horizontalToPlayer = toPlayer;
            horizontalToPlayer.y = 0f;

            if (horizontalToPlayer.sqrMagnitude > attackDistance * attackDistance)
                return;

            if (!HasDirectHitToPlayer(toPlayer))
                return;

            _playerController.TakeDamage(damage);
            _attackTimer = attackRate;
        }
        
        /// <summary>
        /// проверяет что между врагом и игроком нет стены или стола
        /// </summary>
        private bool HasDirectHitToPlayer(Vector3 toPlayer)
        {
            var rayDirection = toPlayer + playerRayOffset - attackRayOffset;
            var rayDistance = rayDirection.magnitude;

            if (rayDistance <= 0.0001f)
                return true;

            var hitCount = Physics.RaycastNonAlloc(
                _transform.position + attackRayOffset,
                rayDirection / rayDistance,
                _attackHits,
                rayDistance,
                attackRaycastMask,
                QueryTriggerInteraction.Ignore);

            if (hitCount <= 0)
                return false;

            if (!TryGetClosestVisibleHit(hitCount, out var hit))
                return false;

            var hitPlayer = hit.collider.GetComponentInParent<PlayerController>();
            return ReferenceEquals(hitPlayer, _playerController);
        }

        private bool TryGetClosestVisibleHit(int hitCount, out RaycastHit closestHit)
        {
            closestHit = default;
            var closestDistance = float.PositiveInfinity;

            for (var i = 0; i < hitCount; i++)
            {
                var hit = _attackHits[i];

                if (hit.collider.transform.IsChildOf(_transform))
                    continue;

                if (hit.distance >= closestDistance)
                    continue;

                closestHit = hit;
                closestDistance = hit.distance;
            }

            return closestDistance < float.PositiveInfinity;
        }
    }
}
