using Interfaces;
using Player;
using UnityEngine;

namespace WorkbenchBuyer
{
    public class BuyerController : MonoBehaviour, IInteractable, ITriggerable
    {
        [SerializeField] private long initialPrice;
        [SerializeField] private float priceMultiplier;
        [SerializeField] private float spawnXOffset;
        [SerializeField] private float spawnYOffset;
        
        [SerializeField] private GameObject workbenchPrefab;

        [SerializeField] private BuyerUI uiComponent;
        


        public void Interact(PlayerInteraction player)
        {
            throw new System.NotImplementedException();
        }

        public IUIPrompts GetUIComponent() => uiComponent;

        public Vector3 GetPosition()
        {
            throw new System.NotImplementedException();
        }

        public void Execute(PlayerController playerController)
        {
            throw new System.NotImplementedException();
        }

        public void Exit(PlayerController playerController)
        {
            throw new System.NotImplementedException();
        }
    }
}
