using Interfaces;
using Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Player
{
    /// <summary>
    /// Имплементирует механику боя для игрока
    /// </summary>
    public class PlayerController : MonoBehaviour, IDamageable
    {
        private const int MAX_RAYCAST_HITS = 16;
        
        [SerializeField] private PlayerInteraction playerInteraction;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private CharacterController charController;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float deathAnimationSpeed;
        [SerializeField] private float attackCooldownRate;
        [SerializeField] private InputActionReference respawnBindings;
        
        [SerializeField] private Camera mainCameraReference;
        [SerializeField] private InputActionReference attackActionReference;
        [SerializeField] private float attackDistance = 20f;
        [SerializeField] private LayerMask raycastLayerMask = ~0;
        [SerializeField] private LayerMask enemyLayerMask;
        
        public int maxHealth;
        public int currentHealth;
        public int damage;

        private bool _isDying;
        private InputAction _respawnAction;
        private GameManager _gameManager;
        private float _attackCooldownTimer;
        
        private readonly RaycastHit[] _raycastHits = new RaycastHit[MAX_RAYCAST_HITS];
        private InputAction _attackAction;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            currentHealth = maxHealth;
            _attackCooldownTimer = attackCooldownRate;
        }

        void Awake()
        {
            _gameManager = GameManager.Instance;
            _respawnAction = respawnBindings.action;
            _attackAction = attackActionReference.action;
        }
        
        void OnEnable()
        {
            _gameManager.OnGameStateStart += OnHomeEnter;
            _respawnAction.Enable();
            _attackAction.Enable();
            _attackAction.performed += OnAttack;
        }

        void OnDisable()
        {
            _gameManager.OnGameStateStart -= OnHomeEnter;
            _respawnAction.Disable();
            _attackAction.Disable();
            _attackAction.performed -= OnAttack;
        }
        
        // Update is called once per frame
        void Update()
        {
            if (_isDying && transform.rotation.x > -0.5f)
                PlayDeathAnimation(deathAnimationSpeed);
            
            if (_attackCooldownTimer > 0)
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
            _attackCooldownTimer = attackCooldownRate;
            
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
            _gameManager.ChangeGameState(GameState.GameOver);
            currentHealth = 0;
            _isDying = true;
            charController.enabled = playerMovement.enabled = playerInteraction.enabled = false;
            _respawnAction.performed += OnRespawn;
        }

        /// <summary>
        /// Проигрывает анимацию смерти - переворачивает игрока на "спину"
        /// </summary>
        /// <param name="rotationSpeed">Скорость вращения</param>
        private void PlayDeathAnimation(float rotationSpeed)
        {
            var currentRot = transform.rotation;
            var targetRot = currentRot * Quaternion.AngleAxis(90f, new Vector3(-90f, currentRot.y, 0f));
            transform.rotation = Quaternion.RotateTowards(currentRot, targetRot, Time.deltaTime * rotationSpeed);
        }
        
        /// <summary>
        /// Возрождает игрока на <c>spawnPoint</c>
        /// </summary>
        private void Respawn()
        {
            _gameManager.ChangeGameState(GameState.Home);
            _isDying = false;
            currentHealth = maxHealth;
            transform.position = spawnPoint.position;
            transform.rotation = Quaternion.identity;
            
            charController.enabled = playerMovement.enabled = playerInteraction.enabled = true;
        }
        
        private void OnAttack(InputAction.CallbackContext context)
        {
            if (!TryGetPointerPosition(context.control, out Vector2 screenPosition, out int pointerId))
                screenPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            
            if (pointerId >= 0 && IsPointerOverUi(pointerId))
                return;
            
            if (pointerId < 0 && IsPointerOverUi())
                return;
            
            AttackAt(screenPosition);
        }

        /// <summary>
        /// выпускает луч и бьет только первого видимого врага
        /// </summary>
        private void AttackAt(Vector2 screenPosition)
        {
            var ray = mainCameraReference.ScreenPointToRay(screenPosition);
            var hitCount = Physics.RaycastNonAlloc(
                ray,
                _raycastHits,
                attackDistance,
                enemyLayerMask,
                QueryTriggerInteraction.Ignore);

            if (!TryGetClosestHit(hitCount, out var hit))
                return;

            var damageable = hit.collider.GetComponentInParent<IDamageable>();
            
            // TODO: проверить, нельзя ли без этого простреливать стены
            // if (damageable is not MonoBehaviour monoBehaviour) 
            //     return;
            //
            // if (!IsInLayerMask(monoBehaviour.gameObject.layer, enemyLayerMask))
            //     return;
            
            damageable.TakeDamage(damage);
        }

        /// <summary>
        /// берет ближайшее попадание не по самому игроку
        /// </summary>
        private bool TryGetClosestHit(int hitCount, out RaycastHit closestHit)
        {
            closestHit = default;
            float closestDistance = float.PositiveInfinity;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = _raycastHits[i];

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
        /// берет позицию указателя если она есть
        /// </summary>
        private static bool TryGetPointerPosition(InputControl control, out Vector2 screenPosition, out int pointerId)
        {
            if (TryGetTouchControl(control, out TouchControl touch))
            {
                screenPosition = touch.position.ReadValue();
                pointerId = touch.touchId.ReadValue();
                return true;
            }

            if (control.device is Pointer pointer)
            {
                screenPosition = pointer.position.ReadValue();
                pointerId = -1;
                return true;
            }

            screenPosition = Vector2.zero;
            pointerId = -1;
            return false;
        }

        /// <summary>
        /// ищет touch control выше по дереву контрола
        /// </summary>
        private static bool TryGetTouchControl(InputControl control, out TouchControl touch)
        {
            for (InputControl current = control; current != null; current = current.parent)
            {
                if (current is TouchControl touchControl)
                {
                    touch = touchControl;
                    return true;
                }
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
        /// проверяет ui для мыши
        /// </summary>
        private static bool IsPointerOverUi()
        {
            return !ReferenceEquals(EventSystem.current, null) && EventSystem.current.IsPointerOverGameObject();
        }

        /// <summary>
        /// проверяет ui для конкретного touch
        /// </summary>
        private static bool IsPointerOverUi(int pointerId)
        {
            return !ReferenceEquals(EventSystem.current, null) && EventSystem.current.IsPointerOverGameObject(pointerId);
        }
    }
}
