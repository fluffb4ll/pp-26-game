using System;
using Interfaces;
using Managers;
using Player;
using Registries;
using Structs;
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
        
        private int _entityIdHash;
        
        private SaveManager _saveManager;
        private UIManager _uiManager;
        private EntityRegistry _entityRegistry;
        
        private Action<long> _onPriceChange;
        private Action<QuestType> _onInteract;
        private Action<Workbench.Workbench> _onBuyWorkbench;

        private void Awake()
        {
            _uiManager = UIManager.Instance;
            _saveManager = SaveManager.Instance;
            _entityRegistry = EntityRegistry.Instance;
            
            _entityIdHash = _entityRegistry.AddBuyer(this);
        }
        
        private void Start()
        {
            _currentPrice = initialPrice;
        }
        
        public event Action<long> OnPriceChange
        {
            add => _onPriceChange += value;
            remove => _onPriceChange -= value;
        }
        
        public event Action<QuestType> OnInteract
        {
            add => _onInteract += value;
            remove => _onInteract -= value;
        }

        public event Action<Workbench.Workbench> OnBuyWorkbench
        {
            add => _onBuyWorkbench += value;
            remove => _onBuyWorkbench -= value;
        }
        
        private void HandleWorkbenchBuying()
        {
            var currentWorkbenchCount = _entityRegistry.GetWorkbenches().Count;
            if (_currentWorkbenchCount == workbenchLimit)
            {
                _uiManager.CreateNotification("Достигнут лимит станков!");
                return;
            }
            
            var coins = _saveManager.GetCoinsAmount();

            if (coins < _currentPrice)
            {
                _uiManager.CreateNotification("Недостаточно слоп-коинов!");
                return;
            }
            
            if (_currentWorkbenchCount != 0 && _currentWorkbenchCount % (workbenchLimit / 2) == 0)
                _rowCount++;
            
            var prefabYAngle = workbenchPrefab.transform.eulerAngles.y;
            var targetYAngle = _rowCount % 2 == 0 ? prefabYAngle + 180f : prefabYAngle;
            
            var pos = transform.position;
            var targetXPos = pos.x + spawnXOffset * 
                (_currentWorkbenchCount + 1 - (_rowCount - 1) * workbenchLimit / 2);
            var targetZPos = _rowCount % 2 == 0 ? pos.z - spawnZOffset : pos.z + spawnZOffset;
            
            var workbench = Instantiate(
                workbenchPrefab, 
                new Vector3(targetXPos, pos.y, targetZPos), 
                Quaternion.Euler(0f, targetYAngle, 0f)).GetComponent<Workbench.Workbench>();
            
            _saveManager.ChangeCoinsAmount(-_currentPrice);
            
            _currentWorkbenchCount++;
            
            _currentPrice = (long) Math.Round(_currentPrice * priceMultiplier);
            _onPriceChange?.Invoke(_currentPrice);
            _onInteract?.Invoke(QuestType.Buy);
            _onBuyWorkbench?.Invoke(workbench);
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
