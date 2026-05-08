using System;
using System.Collections.Generic;
using Interfaces;
using Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR;

namespace Player
{
    /// <summary>
    /// Имплементирует механику боя для игрока
    /// </summary>
    public class PlayerController : MonoBehaviour, IDamageable
    {
        private const int MaxRaycastHits = 16;

        [SerializeField] private PlayerInteraction playerInteraction;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private CharacterController charController;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float deathAnimationSpeed;
        [SerializeField] private float respawnDelay = 1.25f;
        [SerializeField] private float attackCooldownRate;
        [SerializeField] private InputActionReference respawnBindings;
        [SerializeField] private Animator playerModelAnimator;
        
        [SerializeField] private Camera mainCameraReference;
        [SerializeField] private InputActionReference attackActionReference;
        [SerializeField] private float attackDistance = 3f;
        [SerializeField] private float attackRayDistance = 30f;
        [SerializeField] private LayerMask raycastLayerMask = ~0;
        [SerializeField] private LayerMask enemyLayerMask;

        public int maxHealth;
        public int currentHealth;
        public int damage;

        private bool _isDying;
        private InputAction _respawnAction;
        private InputAction _attackAction;
        private GameManager _gameManager;
        private float _attackCooldownTimer;
        private float _respawnTimer;
        private readonly RaycastHit[] _raycastHits = new RaycastHit[MaxRaycastHits];
        private readonly List<RaycastResult> _uiRaycastResults = new(8);
        private EventSystem _uiEventSystem;
        private PointerEventData _uiPointerEventData;
        
        // параметры в PlayerAnimationController
        private const string DeathTransitionFlag = "hasDied";
        private const string WalkingTransitionFlag = "isMoving";
        private const string WalkingSpeedParameter = "walkingSpeed";
        
        private void Awake()
        {
            _gameManager = GameManager.Instance;
            _respawnAction = respawnBindings.action;
            _attackAction = attackActionReference.action;
        }

        
        private void Start()
        {
            currentHealth = maxHealth;
            _attackCooldownTimer = 0f;
        }

        
        private void OnEnable()
        {
            _gameManager.OnGameStateStart += OnHomeEnter;
            _respawnAction.Enable();
            _attackAction.Enable();
            _attackAction.performed += OnAttack;
        }

        
        private void OnDisable()
        {
            _gameManager.OnGameStateStart -= OnHomeEnter;
            _respawnAction.Disable();
            _respawnAction.performed -= OnRespawn;
            _attackAction.performed -= OnAttack;
            _attackAction.Disable();
        }

        
        private void Update()
        {
            UpdateAnimFlags();
            
            if (_isDying)
                HandleRespawnAfterDeath();
            
            if (_attackCooldownTimer > 0f)
                _attackCooldownTimer -= Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            other.TryGetComponent(out ITriggerable triggerable);
            triggerable?.Execute(this);
        }

        /// <summary>
        /// Вызывается при нажатии бинда возрождения
        /// </summary>
        /// <param name="context">Информация о том, что вызвало <c>InputAction</c></param>
        private void OnRespawn(InputAction.CallbackContext context)
        {
            _respawnAction.performed -= OnRespawn;
            Respawn();
        }

        /// <summary>
        /// Лечит игрока при входе в зону <c>Home</c>
        /// </summary>
        /// <param name="newState">Новый геймстейт</param>
        private void OnHomeEnter(GameState newState)
        {
            if (newState == GameState.Home)
                Heal(maxHealth);
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
            if (_isDying)
                return;

            if (playerInteraction.heldBrainrot is not null)
            {
                Destroy(playerInteraction.heldBrainrot.gameObject);
                playerInteraction.heldBrainrot = null;
            }
            
            currentHealth = 0;
            _isDying = true;
            _respawnTimer = respawnDelay;
            _gameManager.ChangeGameState(GameState.GameOver);
            charController.enabled = playerMovement.enabled = playerInteraction.enabled = false;
            _respawnAction.performed += OnRespawn;
            
            UpdateAnimFlags();
        }

        /// <summary>
        /// Обновляет флаги аниматора в зависимости от значений <c>_isDying</c> и параметров ходьбы
        /// </summary>
        private void UpdateAnimFlags()
        {
            playerModelAnimator.SetBool(DeathTransitionFlag, _isDying);
            playerModelAnimator.SetBool(WalkingTransitionFlag, !_isDying && playerMovement.isMoving);
            playerModelAnimator.SetFloat(WalkingSpeedParameter, playerMovement.walkingSpeed);
        }

        private void HandleRespawnAfterDeath()
        {
            _respawnTimer -= Time.deltaTime;
        
            if (_respawnTimer > 0f)
                return;

            OnRespawn(new InputAction.CallbackContext());
        }

        /// <summary>
        /// Возрождает игрока на <c>spawnPoint</c>
        /// </summary>
        private void Respawn()
        {
            _isDying = false;
            _respawnTimer = 0f;
            currentHealth = maxHealth;
            transform.position = spawnPoint.position;
            transform.rotation = Quaternion.identity;

            charController.enabled = playerMovement.enabled = playerInteraction.enabled = true;
            _gameManager.ChangeGameState(GameState.Home);
            
            UpdateAnimFlags();
        }

        /// <summary>
        /// запускает атаку по указателю или по центру экрана
        /// </summary>
        private void OnAttack(InputAction.CallbackContext context)
        {
            if (_isDying || _attackCooldownTimer > 0f)
                return;

            var hasPointer = TryGetPointerPosition(context.control, out var screenPosition);

            if (!hasPointer)
                screenPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            else if (IsPointerOverUi(screenPosition))
                return;

            AttackAt(screenPosition);
            _attackCooldownTimer = attackCooldownRate;
        }

        /// <summary>
        /// выпускает луч и бьёт только первого видимого врага
        /// </summary>
        private void AttackAt(Vector2 screenPosition)
        {
            var ray = mainCameraReference.ScreenPointToRay(screenPosition);
            var hitCount = Physics.RaycastNonAlloc(
                ray,
                _raycastHits,
                attackRayDistance,
                raycastLayerMask,
                QueryTriggerInteraction.Ignore);
            
            
            if (!TryGetClosestHit(hitCount, out var hit))
                return;

            if (!IsInLayerMask(hit.collider.gameObject.layer, enemyLayerMask))
                return;

            if (!IsEnemyInAttackRange(hit.collider))
                return;
            
            var damageable = hit.collider.GetComponentInParent<IDamageable>();

            damageable?.TakeDamage(damage);
        }

        private bool IsEnemyInAttackRange(Collider enemyCollider)
        {
            var playerPosition = transform.position;
            var closestPoint = enemyCollider.ClosestPoint(playerPosition);
            closestPoint.y = playerPosition.y;

            return (closestPoint - playerPosition).sqrMagnitude <= attackDistance * attackDistance;
        }

        /// <summary>
        /// берёт ближайшее попадание не по самому игроку
        /// </summary>
        private bool TryGetClosestHit(int hitCount, out RaycastHit closestHit)
        {
            closestHit = default;
            var closestDistance = float.PositiveInfinity;

            for (var i = 0; i < hitCount; i++)
            {
                var hit = _raycastHits[i];

                if (hit.collider.transform.IsChildOf(transform))
                    continue;

                if (hit.distance >= closestDistance)
                    continue;

                closestHit = hit;
                closestDistance = hit.distance;
            }

            return closestDistance < float.PositiveInfinity;
        }

        /// <summary>
        /// берёт позицию указателя если атака пришла от мыши или touch
        /// </summary>
        private static bool TryGetPointerPosition(InputControl control, out Vector2 screenPosition)
        {
            if (TryGetTouchControl(control, out var touch))
            {
                screenPosition = touch.position.ReadValue();
                return true;
            }

            if (control.device is Pointer pointer)
            {
                screenPosition = pointer.position.ReadValue();
                return true;
            }

            screenPosition = Vector2.zero;
            return false;
        }

        /// <summary>
        /// ищет touch control выше по дереву контрола
        /// </summary>
        private static bool TryGetTouchControl(InputControl control, out TouchControl touch)
        {
            for (var current = control; current != null; current = current.parent)
            {
                if (current is not TouchControl touchControl) 
                    continue;
                
                touch = touchControl;
                return true;
            }

            touch = null;
            return false;
        }

        /// <summary>
        /// проверяет слой через маску
        /// </summary>
        private static bool IsInLayerMask(int layer, LayerMask layerMask)
        {
            return (layerMask.value & (1 << layer)) != 0;
        }

        /// <summary>
        /// проверяет попал ли клик или тап в ui прямо сейчас
        /// </summary>
        private bool IsPointerOverUi(Vector2 screenPosition)
        {
            if (ReferenceEquals(EventSystem.current, null))
                return false;

            if (ReferenceEquals(_uiPointerEventData, null) || !ReferenceEquals(_uiEventSystem, EventSystem.current))
            {
                _uiEventSystem = EventSystem.current;
                _uiPointerEventData = new PointerEventData(_uiEventSystem);
            }

            _uiPointerEventData.Reset();
            _uiPointerEventData.position = screenPosition;

            _uiRaycastResults.Clear();
            _uiEventSystem.RaycastAll(_uiPointerEventData, _uiRaycastResults);
            return _uiRaycastResults.Count > 0;
        }
    }
}
