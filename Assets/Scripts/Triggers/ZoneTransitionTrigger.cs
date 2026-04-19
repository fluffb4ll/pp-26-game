using Interfaces;
using Managers;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Triggers
{
    /// <summary>
    /// Триггер смены зоны
    /// </summary>
    public class ZoneTransitionTrigger : MonoBehaviour, ITriggerable
    {
        public GameState targetState;
        [SerializeField] private InputActionReference respawnAction;
        
        private GameManager _gameManager;
        private GameState _initialGameState;
        private InputAction _respawnAction;

        private void Awake()
        {
            _initialGameState = targetState;
            _respawnAction = respawnAction.action;
            _gameManager = GameManager.Instance;
        }

        private void OnEnable()
        {
            _respawnAction.performed += ResetTargetGameState;
        }

        private void OnDisable()
        {
            _respawnAction.performed -= ResetTargetGameState;
        }
        
        /// <inheritdoc/>
        public void Execute(PlayerController playerController)
        {
            GameState tempState = targetState;
            targetState = _gameManager.currentState;
            _gameManager.ChangeGameState(tempState);
        }
        
        /// <summary>
        /// Восстанавливает целевой геймстейт при респавне или ином действии
        /// </summary>
        /// <param name="context">Информация о том, что вызвало <c>InputAction</c></param>
        public void ResetTargetGameState(InputAction.CallbackContext context)
        {
            targetState = _initialGameState;
        }
    }
}
