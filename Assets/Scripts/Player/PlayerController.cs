using Interfaces;
using Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    /// <summary>
    /// Имплементирует механику боя для игрока
    /// </summary>
    public class PlayerController : MonoBehaviour, IDamageable
    {
        [SerializeField] private PlayerInteraction playerInteraction;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private CharacterController charController;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float deathAnimationSpeed;
        [SerializeField] private InputActionReference respawnBindings;
        
        public GameManager gameManager;
        public int maxHealth;
        public int damage;
        public int health;

        private bool _isDying;
        private InputAction _respawnAction;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _respawnAction = respawnBindings.action;
            health = maxHealth;
        }
        
        void OnEnable()
        {
            gameManager.OnGameStateStart += OnHomeEnter;
        }

        void OnDisable()
        {
            gameManager.OnGameStateStart -= OnHomeEnter;
        }
        
        // Update is called once per frame
        void Update()
        {
            if (_isDying && transform.rotation.x > -0.5f)
                PlayDeathAnimation(deathAnimationSpeed);
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
            health -= damageAmount;
            
            if (health <= 0)
                Die();
        }
        
        /// <inheritdoc/>
        public void Heal(int healAmount)
        {
            health += healAmount;
            
            if (health > maxHealth)
                health = maxHealth;
        }

        /// <inheritdoc/>
        public void Die()
        {
            gameManager.ChangeGameState(GameState.GameOver);
            health = 0;
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
            Quaternion currentRot = transform.rotation;
            Quaternion targetRot = currentRot * Quaternion.AngleAxis(90f, new Vector3(-90f, currentRot.y, 0f));
            transform.rotation = Quaternion.RotateTowards(currentRot, targetRot, Time.deltaTime * rotationSpeed);
        }
        
        /// <summary>
        /// Возрождает игрока на <c>spawnPoint</c>
        /// </summary>
        private void Respawn()
        {
            gameManager.ChangeGameState(GameState.Home);
            _isDying = false;
            health = maxHealth;
            transform.position = spawnPoint.position;
            transform.rotation = Quaternion.identity;
            
            charController.enabled = playerMovement.enabled = playerInteraction.enabled = true;
        }
    }
}
