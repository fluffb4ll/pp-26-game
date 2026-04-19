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
        [SerializeField] private GameObject spawnableBrainrot;
        [SerializeField] private float timeBeforeDestroyingBody;
        [SerializeField] private float deathAnimationSpeed;

        [SerializeField] private int maxHealth;
        [SerializeField] private int currentHealth;
        [SerializeField] private int damage;
        [SerializeField] private float attackRate;
        [SerializeField] private float attackDistance;
        [SerializeField] private Vector3 attackRayOffset = new Vector3(0f, 0.6f, 0f);
        [SerializeField] private Vector3 playerRayOffset = new Vector3(0f, 0.9f, 0f);
        [SerializeField] private LayerMask attackRaycastMask = ~0;

        public SpawnManager spawnManager;

        private GameManager _gameManager;
        private Transform _playerTransform;
        private Transform _transform;
        private bool _isDying;
        private float _attackTimer;
        private PlayerController _playerController;
        private EnemyMovement _enemyMovement;

        /// <summary>
        /// кешируем нужные ссылки один раз
        /// </summary>
        private void Awake()
        {
            currentHealth = maxHealth;
            _gameManager = GameManager.Instance;
            _playerTransform = _gameManager.playerTransform;
            _playerController = _gameManager.playerController;
            _enemyMovement = GetComponent<EnemyMovement>();
            _transform = transform;
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

            if (!ReferenceEquals(_enemyMovement, null))
                _enemyMovement.enabled = false;

            if (!ReferenceEquals(spawnManager, null))
                spawnManager.UnregisterEnemy(gameObject);

            SpawnBrainrot();
        }

        /// <summary>
        /// проигрывает простую анимацию смерти через поворот
        /// </summary>
        private void PlayDeathAnimation()
        {
            Quaternion currentRot = _transform.rotation;
            Quaternion targetRot = currentRot * Quaternion.AngleAxis(90f, new Vector3(-90f, currentRot.y, 0f));
            _transform.rotation =
                Quaternion.RotateTowards(
                    currentRot,
                    targetRot,
                    Time.deltaTime * deathAnimationSpeed);
        }

        /// <summary>
        /// спавнит награду после смерти врага
        /// </summary>
        private void SpawnBrainrot()
        {
            if (!ReferenceEquals(spawnableBrainrot, null))
                Instantiate(spawnableBrainrot, _transform.position, Quaternion.identity);
        }

        /// <summary>
        /// проигрывает смерть и потом удаляет тело
        /// </summary>
        private void HandleDeath()
        {
            if (_transform.rotation.x > -0.5f)
                PlayDeathAnimation();
            else if (timeBeforeDestroyingBody > 0f)
                timeBeforeDestroyingBody -= Time.deltaTime;
            else
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

            if (!IsPlayerInAttackRange() || !HasDirectHitToPlayer())
                return;

            _playerController.TakeDamage(damage);
            _attackTimer = attackRate;
        }

        /// <summary>
        /// проверяет настоящую 3d дистанцию до игрока
        /// </summary>
        private bool IsPlayerInAttackRange()
        {
            var toPlayer = _playerTransform.position - _transform.position;
            return toPlayer.sqrMagnitude <= attackDistance * attackDistance;
        }

        /// <summary>
        /// проверяет что между врагом и игроком нет стены или стола
        /// </summary>
        private bool HasDirectHitToPlayer()
        {
            var origin = _transform.position + attackRayOffset;
            var target = _playerTransform.position + playerRayOffset;
            var rayDirection = target - origin;
            var rayDistance = rayDirection.magnitude;

            if (rayDistance <= 0.0001f)
                return true;

            if (!Physics.Raycast(origin, rayDirection / rayDistance, out var hit, rayDistance, attackRaycastMask, QueryTriggerInteraction.Ignore))
                return false;

            var hitPlayer = hit.collider.GetComponentInParent<PlayerController>();
            return ReferenceEquals(hitPlayer, _playerController);
        }
    }
}
