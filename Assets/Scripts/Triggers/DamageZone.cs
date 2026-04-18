using Interfaces;
using Player;
using UnityEngine;

namespace Triggers
{
    public class DamageZone : MonoBehaviour, ITriggerable
    {
        public int damage;

        public void Execute(PlayerController playerController)
        {
            playerController.TakeDamage(damage);
        }
    }
}
