using System;
using System.Collections.Generic;
using Interfaces;
using Brainrot;
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

        private void Awake()
        {
            _playerMovement = PlayerMovement.Instance;
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
        }

        /// <summary>
        /// Метод, вызываемый при отключении объекта
        /// </summary>
        private void OnDisable()
        {
            _interactAction.Disable();
            _interactAction.performed -= OnInteract;
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
        /// Прекращает отслеживать объект, с которым можно взаимодействовать, из списка активных
        /// </summary>
        /// <param name="interactable">Объект, реализующий интерфейс <see cref="IInteractable"/></param>
        public void UnregisterInteractable(IInteractable interactable)
        {
            interactable.GetUIComponent().HidePrompts();
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
            if (_activeInteractables.Count <= 0 || !_hasUpdatedInteractables) return;
            
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
                _currentInteractable?.GetUIComponent().HidePrompts();
                _currentInteractable = closestTarget;
                _currentInteractable?.GetUIComponent().ShowPrompts();
            }

            _hasUpdatedInteractables = false;
        }
        
        /// <summary>
        /// Телепортирует игрока в указанную точку
        /// </summary>
        /// <param name="target">Точку, в которую нужно телепортировать игрока</param>
        public void TeleportPlayer(Transform target) => _playerMovement.TeleportPlayer(target);
    }
}
