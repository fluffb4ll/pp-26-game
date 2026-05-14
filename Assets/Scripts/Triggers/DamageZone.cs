using Interfaces;
using Player;
using UnityEngine;

namespace Triggers
{
    /// <summary>
    /// Триггер получения урона
    /// </summary>
    public class DamageZone : MonoBehaviour, ITriggerable
    {
        public int damage;

        /// <inheritdoc/>
        public void Execute(PlayerController playerController)
        {
            playerController.TakeDamage(damage);
        }
        
        /// <inheritdoc/>
        public void Exit(PlayerController playerController) {}
    }
}
