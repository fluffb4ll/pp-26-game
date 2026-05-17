using System;
using Interfaces;
using Managers;
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
        [SerializeField] private float spawnZOffset;
        
        [SerializeField] private int workbenchLimit;
        
        [SerializeField] private GameObject workbenchPrefab;

        [SerializeField] private InteractableUI uiComponent;

        private long _currentPrice;
        private int _currentWorkbenchCount;
        private int _rowCount = 1;
        
        private GameManager _gameManager;
        
        private Action<long> _onPriceChange;

        private void Start()
        {
            _gameManager = GameManager.Instance;
            _currentPrice = initialPrice;
        }
        
        public event Action<long> OnPriceChange
        {
            add => _onPriceChange += value;
            remove => _onPriceChange -= value;
        }

        private void HandleWorkbenchBuying()
        {
            // TODO: добавить уведомления
            if (_currentWorkbenchCount == workbenchLimit)
                throw new Exception("Workbench limit reached");
            
            var coins = _gameManager.GetCoinsAmount();

            // TODO: добавить уведомления
            if (coins < _currentPrice)
                throw new Exception("Not enough coins");
            
            if (_currentWorkbenchCount != 0 && _currentWorkbenchCount % (workbenchLimit / 2) == 0)
                _rowCount++;
            
            var prefabYAngle = workbenchPrefab.transform.eulerAngles.y;
            var targetYAngle = _rowCount % 2 == 0 ? prefabYAngle + 180f : prefabYAngle;
            
            var pos = transform.position;
            var targetXPos = pos.x + spawnXOffset * 
                (_currentWorkbenchCount + 1 - (_rowCount - 1) * workbenchLimit / 2);
            var targetZPos = _rowCount % 2 == 0 ? pos.z - spawnZOffset : pos.z + spawnZOffset;
            
            Instantiate(
                workbenchPrefab, 
                new Vector3(targetXPos, pos.y, targetZPos), 
                Quaternion.Euler(0f, targetYAngle, 0f));
            
            _gameManager.ChangeCoinsAmount(-_currentPrice);
            
            _currentWorkbenchCount++;
            
            _currentPrice = (long) Math.Round(_currentPrice * priceMultiplier);
            _onPriceChange?.Invoke(_currentPrice);
        }
        
        public void Interact(PlayerInteraction player)
        {
            HandleWorkbenchBuying();
        }
        
        public IUIPrompts GetUIComponent() => uiComponent;

        public Vector3 GetPosition() => transform.position;

        public void Execute(PlayerController playerController)
        {
            playerController.GetPlayerInteraction().RegisterInteractable(this);
        }

        public void Exit(PlayerController playerController)
        {
            playerController.GetPlayerInteraction().UnregisterInteractable(this);
        }
        
        public long GetCurrentPrice() => _currentPrice;
    }
}
