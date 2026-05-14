using System.Collections.Generic;
using Interfaces;
using Managers;
using Player;
using UnityEngine;

namespace Boat
{
    public class BoatController : MonoBehaviour, IInteractable, ITriggerable
    {
        [SerializeField] private List<Transform> boatPositions;
        [SerializeField] private List<Transform> playerPositions;
        [SerializeField] private bool isAtHome = true;
        
        [SerializeField] private BoatUI uiComponent;
        
        private GameManager _gameManager;

        private void Awake()
        {
            _gameManager = GameManager.Instance;
        }

        /// <summary>
        /// Перемещает лодку и игрока на другой остров
        /// </summary>
        /// <param name="player">Компонент <see cref="PlayerInteraction"/> игрока, вызвавшего взаимодействие</param>
        private void Travel(PlayerInteraction player)
        {
            var transformIndex = isAtHome ? 1 : 0; 
            var targetTransform = boatPositions[transformIndex];
            transform.position = targetTransform.position;
            transform.rotation = targetTransform.rotation;
            
            player.TeleportPlayer(playerPositions[transformIndex]);
            _gameManager.ChangeGameState(isAtHome ? GameState.Combat : GameState.Home);
            
            player.UnregisterInteractable(this);
            isAtHome = !isAtHome;
        }
        
        /// <inheritdoc/>
        public void Interact(PlayerInteraction player)
        {
            Travel(player);
        }
        
        /// <inheritdoc/>
        public IUIPrompts GetUIComponent() => uiComponent;

        /// <inheritdoc/>
        public Vector3 GetPosition() => transform.position;

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
    }
}
