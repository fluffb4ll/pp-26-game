using System;
using System.Collections.Generic;
using Interfaces;
using Brainrot;
using Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    /// <summary>
    /// Имплементирует методы взаимодействия игрока с другими объектами
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private InputActionReference interact;
    
        public Transform brainrotCarryPoint;
        public BrainrotObject heldBrainrot;

        private InputAction _interactAction;
        private List<IInteractable> _activeInteractables = new();
        private IInteractable _currentInteractable;
        private bool _hasUpdatedInteractables;
        
        private PlayerMovement _playerMovement;
        private PlayerController _playerController;
        private GameManager _gameManager;
        private UIManager _uiManager;

        private void Awake()
        {
            _playerMovement = PlayerMovement.Instance;
            _gameManager = GameManager.Instance;
            _uiManager = UIManager.Instance;
            _playerController = _gameManager.playerController;
        }
        
        private void Start()
        {
            _interactAction ??= interact.action;
        }

        private void Update()
        {
            CalculateInteractableScore();
        }
        
        /// <summary>
        /// Метод, вызываемый при включении объекта
        /// </summary>
        private void OnEnable()
        {
            if (_interactAction is null)
                _interactAction = interact.action;
            _interactAction.Enable();
            _interactAction.performed += OnInteract;
            
            _playerController.OnRespawn += ClearInteractables;
        }

        /// <summary>
        /// Метод, вызываемый при отключении объекта
        /// </summary>
        private void OnDisable()
        {
            _interactAction.Disable();
            _interactAction.performed -= OnInteract;
            
            _playerController.OnRespawn -= ClearInteractables;
        }

        /// <summary>
        /// Действие, которое необходимо выполнять при нажатии кнопки взаимодействия
        /// </summary>
        /// <param name="context">Информация о том, что вызвало <c>InputAction</c></param>
        private void OnInteract(InputAction.CallbackContext context)
        {
            _currentInteractable?.Interact(this);
        }
        
        /// <summary>
        /// Начинает отслеживать объект, с которым можно взаимодействовать
        /// </summary>
        /// <param name="interactable">Объект, реализующий интерфейс <see cref="IInteractable"/></param>
        public void RegisterInteractable(IInteractable interactable)
        {
            _activeInteractables.Add(interactable);
            _hasUpdatedInteractables = true;
        }
        
        /// <summary>
        /// Прекращает отслеживать объект, с которым можно взаимодействовать
        /// </summary>
        /// <param name="interactable">Объект, реализующий интерфейс <see cref="IInteractable"/></param>
        public void UnregisterInteractable(IInteractable interactable)
        {
            interactable.GetUIComponent().HideInteractionPrompts();
            _activeInteractables.Remove(interactable);
            if (_currentInteractable == interactable)
                _currentInteractable = null;
            _hasUpdatedInteractables = true;
        }
        
        /// <summary>
        /// Считает "счёт" элементов, с которыми можно взаимодействовать, чтобы определить,
        /// какой элемент стоит выбрать активным и отобразить его промпты взаимодействия.
        /// </summary>
        private void CalculateInteractableScore()
        {
            if (!_hasUpdatedInteractables 
                || _gameManager.currentState == GameState.GameOver) 
                return;

            if (_activeInteractables.Count == 0)
            {
                _uiManager.ToggleInteractButton(false);
                return;
            }
            
            IInteractable closestTarget = null;
            var maxPriority = -1f;
            
            foreach (var interactable in _activeInteractables)
            {
                var directionToItem = (interactable.GetPosition() - transform.position).normalized;
                var lookAlignment = Vector3.Dot(transform.forward, directionToItem);

                if (!(lookAlignment > maxPriority)) continue;
                
                maxPriority = lookAlignment;
                closestTarget = interactable;
            }

            if (_currentInteractable != closestTarget)
            {
                _currentInteractable?.GetUIComponent().HideInteractionPrompts();
                _currentInteractable = closestTarget;
                _currentInteractable?.GetUIComponent().ShowInteractionPrompts();
            }
            
            _hasUpdatedInteractables = false;
            
            _uiManager.ToggleInteractButton(_currentInteractable is not null);
        }
        
        /// <summary>
        /// Телепортирует игрока в указанную точку
        /// </summary>
        /// <param name="target">Точку, в которую нужно телепортировать игрока</param>
        public void TeleportPlayer(Transform target) => _playerMovement.TeleportPlayer(target);
        
        /// <summary>
        /// Очищает список объектов, с которыми можно взаимодействовать
        /// </summary>
        private void ClearInteractables()
        {
            foreach (var interactable in _activeInteractables)
                interactable.GetUIComponent().HideInteractionPrompts();
            _activeInteractables.Clear();
            _currentInteractable = null;
            _uiManager.ToggleInteractButton(false);
        }
    }
}
