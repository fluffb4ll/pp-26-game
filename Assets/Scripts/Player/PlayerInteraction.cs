using System;
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
        [SerializeField] private float interactionDistance = 3f;
        [SerializeField] private InputActionReference interact;
    
        public Transform brainrotCarryPoint;
        public BrainrotObject heldBrainrot;

        private InputAction _interactAction;
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _interactAction ??= interact.action;
        }

        /// <summary>
        /// Метод, вызываемый при включении объекта
        /// </summary>
        void OnEnable()
        {
            if (_interactAction is null)
                _interactAction = interact.action;
            _interactAction.Enable();
            _interactAction.performed += OnInteract;
        }

        /// <summary>
        /// Метод, вызываемый при отключении объекта
        /// </summary>
        void OnDisable()
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
            PerformRaycast((hit) =>
            {
                var interactable = hit.collider.GetComponentInParent<IInteractable>();
                interactable?.Interact(this);
            }, interactionDistance);
        }
    
        /// <summary>
        /// Бросает луч и выполняет метод, указанный в <c>callback</c>
        /// </summary>
        /// <param name="callback">Метод, выполняемый при попадании луча в цель</param>
        /// <param name="raycastDistance">Дальность броска луча</param>
        private void PerformRaycast(Action<RaycastHit> callback, float raycastDistance)
        {
            var ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out var hit, raycastDistance))
                callback.Invoke(hit);
        }
    }
}
