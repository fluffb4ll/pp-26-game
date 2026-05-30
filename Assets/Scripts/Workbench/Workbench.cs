using System;
using Brainrot;
using Helpers;
using Interfaces;
using Managers;
using Player;
using Registries;
using Structs;
using UI;
using UnityEngine;

namespace Workbench
{
    /// <summary>
    /// Представляет станок, добывающий монетки 
    /// </summary>
    public class Workbench : MonoBehaviour, IInteractable, ITriggerable
    {
        public float baseProduce;
        public float produceStoreCap;
        public float storedProduce;
        
        [SerializeField] private Transform brainrotInsertionPos;
        [SerializeField] private BrainrotObject insertedBrainrot;
        
        private const float FloatDiff = 0.0001f;

        private string _entityId;
        private int _entityIdHash;
        
        private float _currentProduceRate;
        private SaveManager _saveManager;

        private Action<float> _onProduceUpdate;
        private Action<Workbench> _onBrainrotInsertion;
        private Action<float> _onBrainrotLifeTimeUpdate;
        private Action<float> _onProduceRateUpdate;
        private Action<Workbench> _onBrainrotDeath;
        private Action<QuestType> _onInteract;
        private Action<Workbench> _onCollectCoins;
        
        [SerializeField] private InteractableUI uiComponent;
        
        private void Awake()
        {
            _saveManager = SaveManager.Instance;   
            
            EntityRegistry.Instance.AddWorkbench(this);
        }
        
        private void Update()
        {
            CalculateProduce();
        }
        
        public event Action<float> OnProduceUpdate
        {
            add => _onProduceUpdate += value;
            remove => _onProduceUpdate -= value;
        }
        
        public event Action<Workbench> OnBrainrotInsertion
        {
            add => _onBrainrotInsertion += value;
            remove => _onBrainrotInsertion -= value;
        }
        
        public event Action<float> OnBrainrotLifeTimeUpdate
        {
            add => _onBrainrotLifeTimeUpdate += value;
            remove => _onBrainrotLifeTimeUpdate -= value;
        }
        
        public event Action<float> OnProduceRateUpdate
        {
            add => _onProduceRateUpdate += value;
            remove => _onProduceRateUpdate -= value;
        }
        
        public event Action<Workbench> OnBrainrotDeath
        {
            add => _onBrainrotDeath += value;
            remove => _onBrainrotDeath -= value;
        }
    
        public event Action<QuestType> OnInteract
        {
            add => _onInteract += value;
            remove => _onInteract -= value;
        }
        
        public event Action<Workbench> OnCollectCoins
        {
            add => _onCollectCoins += value;
            remove => _onCollectCoins -= value;
        }
        
        /// <summary>
        /// Вырабатывает монетки за единицу времени и снижает ресурс вставленного брейнрота 
        /// </summary>
        private void CalculateProduce()
        {
            if (insertedBrainrot is null || produceStoreCap - storedProduce < FloatDiff)
                return;
        
            storedProduce += _currentProduceRate * Time.deltaTime;
            
            if (storedProduce > produceStoreCap)
                storedProduce = produceStoreCap;
            _onProduceUpdate?.Invoke(storedProduce);
            
            insertedBrainrot.lifetime -= Time.deltaTime;
            _onBrainrotLifeTimeUpdate?.Invoke(insertedBrainrot.lifetime);
            
            if (!(insertedBrainrot.lifetime <= 0))
                return;
        
            Destroy(insertedBrainrot.gameObject);
            insertedBrainrot = null;
            _currentProduceRate = baseProduce;
            
            _onBrainrotDeath?.Invoke(this);
        }

        /// <summary>
        /// Обрабатывает взаимодействие с игроком
        /// </summary>
        /// <param name="player">Компонент <see cref="PlayerInteraction"/> игрока, вызвавшего взаимодействие</param>
        public void Interact(PlayerInteraction player)
        {
            if (player.heldBrainrot is not null && insertedBrainrot is null)
            {
                InsertBrainrot(player);
                return;
            }
            
            _saveManager.ChangeCoinsAmount((long) Math.Round(storedProduce));
            storedProduce = 0;
            _onProduceUpdate?.Invoke(storedProduce);
            _onInteract?.Invoke(QuestType.Collect);
            _onCollectCoins?.Invoke(this);
        }

        /// <summary>
        /// Устанавливает брейнрота на станок
        /// </summary>
        /// <param name="player">Компонент <see cref="PlayerInteraction"/> игрока, вызвавшего взаимодействие</param>
        private void InsertBrainrot(PlayerInteraction player)
        {
            insertedBrainrot = player.heldBrainrot;
            player.heldBrainrot = null;
            
            insertedBrainrot.transform.SetParent(brainrotInsertionPos);
            insertedBrainrot.transform.position = brainrotInsertionPos.position;
            
            _currentProduceRate += insertedBrainrot.produce;
            
            _onBrainrotInsertion?.Invoke(this);
            _onProduceRateUpdate?.Invoke(_currentProduceRate);
            _onInteract?.Invoke(QuestType.Insert);
        }
        
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
        
        /// <inheritdoc/>
        public IUIPrompts GetUIComponent() => uiComponent;
        
        /// <inheritdoc/>
        public Vector3 GetPosition() => transform.position;

        public BrainrotObject GetInsertedBrainrot() => insertedBrainrot;
        
        /// <summary>
        /// Рассчитывает количество монет, которое было выработано за время отсутствия игрока
        /// </summary>
        /// <param name="timeDelta">Разница во времени между последним и нынешним логинами в миллисекундах</param>
        public void CalculateOfflineWork(long timeDelta)
        {
            var seconds = timeDelta / 1000;
            if (insertedBrainrot is null)
                return;
            
            var delta = _currentProduceRate;
            
            if (seconds > insertedBrainrot.lifetime)
            {
                delta *= insertedBrainrot.lifetime;
                Destroy(insertedBrainrot.gameObject);
                insertedBrainrot = null;
                _currentProduceRate = baseProduce;
                _onBrainrotDeath?.Invoke(this);
            }
            else
                delta *= seconds;
            
            storedProduce = delta + storedProduce <= produceStoreCap ? delta + storedProduce : produceStoreCap;
        }

        public void LoadSavedData(float baseProduce, float produceStoreCap, float storedProduce)
        {
            this.baseProduce = baseProduce;
            this.produceStoreCap = produceStoreCap;
            this.storedProduce = storedProduce;
        }

        public void LoadBrainrotData(float produce, float lifetime, BrainrotType type, Rarity rarity)
        {
            insertedBrainrot = Instantiate(EntityRegistry.Instance.FindBrainrotPrefab(type), Vector3.zero, Quaternion.identity, brainrotInsertionPos.transform)
                .GetComponent<BrainrotObject>();
            insertedBrainrot.rarity = rarity;
            insertedBrainrot.lifetime = lifetime;
            insertedBrainrot.produce = produce;
            
            _currentProduceRate = baseProduce + produce;
            _onBrainrotInsertion?.Invoke(this);
        }

        public int GetEntityIdHash() => _entityIdHash;
        
        public void SetEntityIdHash(int hash) => _entityIdHash = hash;
    }
}
