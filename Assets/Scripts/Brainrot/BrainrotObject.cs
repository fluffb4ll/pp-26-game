using System;
using Interfaces;
using Player;
using UI;
using UnityEngine;

namespace Brainrot
{
    /// <summary>
    /// Представляет объект-брейнрот, используемый для добычи ресурсов
    /// </summary>
    public class BrainrotObject : MonoBehaviour, IInteractable, ITriggerable
    {
        public float produce;
        public float lifetime;
        [SerializeField] private BrainrotLib data;
        public Rarity rarity;
        
        [SerializeField] private InteractableUI uiComponent;

        private Action _onBrainrotRoll;
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            var rolledConfig = data.GetRandomizedRarity();
            
            rarity = rolledConfig.rarity;
            produce = data.baseProduce * rolledConfig.produceMult;
            lifetime = data.baseLifetime * rolledConfig.lifetimeMult;
            _onBrainrotRoll?.Invoke();
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"Pulled: {rarity} {data.type}.");
            #endif
        }

        public event Action OnBrainrotRoll
        {
            add => _onBrainrotRoll += value;
            remove => _onBrainrotRoll -= value;
        }
        
        /// <summary>
        /// Обрабатывает взаимодействие игрока с брейнротом
        /// </summary>
        /// <param name="player">Компонент <see cref="PlayerInteraction"/> игрока, вызвавшего взаимодействие</param>
        public void Interact(PlayerInteraction player)
        {
            if (player.heldBrainrot is null)
                PickUp(player);
        }
        
        /// <summary>
        /// Обрабатывает подъём брейнрота игроком
        /// </summary>
        /// <param name="player">Компонент <see cref="PlayerInteraction"/> игрока, вызвавшего взаимодействие</param>
        private void PickUp(PlayerInteraction player)
        {
            player.heldBrainrot = this;
        
            transform.SetParent(player.brainrotCarryPoint);
            transform.localPosition = Vector3.zero;
            
            player.UnregisterInteractable(this);
            uiComponent.DisableUIComponents();
        }
        
        /// <inheritdoc/>
        public void Execute(PlayerController playerController)
        {
            playerController.GetPlayerInteraction().RegisterInteractable(this);
        }
        
        /// <inheritdoc/>
        public void Exit(PlayerController playerController)
        {
            playerController.GetPlayerInteraction().UnregisterInteractable(this);
        }
        
        /// <inheritdoc/>
        public IUIPrompts GetUIComponent() => uiComponent;
        
        /// <inheritdoc/>
        public Vector3 GetPosition() => transform.position;
    }
}
