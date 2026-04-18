using Interfaces;
using Managers;
using Player;
using UnityEngine;

namespace Triggers
{
    public class ZoneChangeTrigger : MonoBehaviour, ITriggerable
    {
        public GameState targetState;

        public void Execute(PlayerController playerController)
        {
            GameState tempState = targetState;
            targetState = playerController.gameManager.currentState;
            playerController.gameManager.ChangeGameState(tempState);
        }
    }
}
