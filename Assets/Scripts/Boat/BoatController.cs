using System.Collections.Generic;
using Interfaces;
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
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void MoveBoat(PlayerInteraction player)
        {
            var targetTransform = boatPositions[isAtHome ? 1 : 0];
            transform.position = targetTransform.position;
            transform.rotation = targetTransform.rotation;
            
            player.TeleportPlayer(playerPositions[isAtHome ? 1 : 0]);
            isAtHome = !isAtHome;
        }
    
        public void Interact(PlayerInteraction player)
        {
            MoveBoat(player);
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
