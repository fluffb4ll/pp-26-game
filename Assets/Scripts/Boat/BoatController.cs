using System.Collections.Generic;
using Interfaces;
using Managers;
using Player;
using UI;
using UnityEngine;

namespace Boat
{
    public class BoatController : MonoBehaviour, IInteractable, ITriggerable
    {
        [SerializeField] private List<Transform> boatPositions;
        [SerializeField] private List<Transform> playerPositions;
        [SerializeField] private bool isAtHome = true;
        
        [SerializeField] private InteractableUI uiComponent;
        
        private GameManager _gameManager;
        private PlayerController _playerController;
        private void Awake()
        {
            _gameManager = GameManager.Instance;
            _playerController = _gameManager.playerController;
        }

        private void OnEnable()
        {
            _playerController.OnRespawn += ResetBoat;
        }

        private void OnDisable()
        {
            _playerController.OnRespawn -= ResetBoat;
        }
        
        /// <summary>
        /// Возвращает лодку в домашнюю зону
        /// </summary>
        private void ResetBoat()
        {
            isAtHome = true;
            MoveBoat(0);
        }
        
        /// <summary>
        /// Перемещает лодку и игрока на другой остров
        /// </summary>
        /// <param name="player">Компонент <see cref="PlayerInteraction"/> игрока, вызвавшего взаимодействие</param>
        private void Travel(PlayerInteraction player)
        {
            var transformIndex = isAtHome ? 1 : 0; 
            MoveBoat(transformIndex);
            
            player.TeleportPlayer(playerPositions[transformIndex]);
            _gameManager.ChangeGameState(isAtHome ? GameState.Combat : GameState.Home);
            
            player.UnregisterInteractable(this);
            isAtHome = !isAtHome;
        }
        
        /// <summary>
        /// Перемещает лодку на указанную позицию из списка <c>boatPositions</c>
        /// </summary>
        /// <param name="transformIndex">Индекс конечной позиции в списке <c>boatPositions</c></param>
        private void MoveBoat(int transformIndex)
        {
            var targetTransform = boatPositions[transformIndex];
            transform.position = targetTransform.position;
            transform.rotation = targetTransform.rotation;
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
