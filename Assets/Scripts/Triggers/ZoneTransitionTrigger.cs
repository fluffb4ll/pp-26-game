using Interfaces;
using Managers;
using Player;
using UnityEngine;

namespace Triggers
{
    /// <summary>
    /// Триггер смены зоны
    /// </summary>
    public class ZoneTransitionTrigger : MonoBehaviour, ITriggerable
    {
        public GameState targetState;

        /// <inheritdoc/>
        public void Execute(PlayerController playerController)
        {
            GameState tempState = targetState;
            targetState = playerController.gameManager.currentState;
            playerController.gameManager.ChangeGameState(tempState);
        }
    }
}
