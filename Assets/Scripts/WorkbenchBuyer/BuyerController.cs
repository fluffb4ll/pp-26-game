using System;
using Interfaces;
using Player;
using UI;
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

        [SerializeField] private InteractableUI uiComponent;

        private long _currentPrice;
        
        private Action<long> _onPriceChange;

        private void Start()
        {
            
        }
        
        public event Action<long> OnPriceChange
        {
            add => _onPriceChange += value;
            remove => _onPriceChange -= value;
        }
        
        public void Interact(PlayerInteraction player)
        {
            throw new System.NotImplementedException();
        }

        public IUIPrompts GetUIComponent() => uiComponent;

        public Vector3 GetPosition() => transform.position;

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
