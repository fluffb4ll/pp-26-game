using System;
using Brainrot;
using Interfaces;
using Managers;
using Player;
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
        private GameManager _gameManager;

        private Action _onProduceUpdate;
        
        [SerializeField] private WorkbenchUI uiComponent;
        
        private void Awake()
        {
            _gameManager = GameManager.Instance;    
        }
        
        private void Update()
        {
            CalculateProduce();
        }
        
        public event Action OnProduceUpdate
        {
            add => _onProduceUpdate += value;
            remove => _onProduceUpdate -= value;
        }
    
        /// <summary>
        /// Вырабатывает монетки за единицу времени и снижает ресурс вставленного брейнрота 
        /// </summary>
        private void CalculateProduce()
        {
            if (insertedBrainrot is null || produceStoreCap - storedProduce < FloatDiff)
                return;
        
            storedProduce += (baseProduce + insertedBrainrot.produce) * Time.deltaTime;
            
            if (storedProduce > produceStoreCap)
                storedProduce = produceStoreCap;
            _onProduceUpdate?.Invoke();
            
            insertedBrainrot.lifetime -= Time.deltaTime;
            
            if (!(insertedBrainrot.lifetime <= 0))
                return;
        
            Destroy(insertedBrainrot.gameObject);
            insertedBrainrot = null;
        }

        /// <summary>
        /// Обрабатывает взаимодействие с игроком
        /// </summary>
        /// <param name="player">Компонент <see cref="PlayerInteraction"/> игрока, вызвавшего взаимодействие</param>
        public void Interact(PlayerInteraction player)
        {
            if (player.heldBrainrot is not null && insertedBrainrot is null)
                InsertBrainrot(player);
            else
            {
                _gameManager.ChangeCoinsAmount(Mathf.RoundToInt(storedProduce));
                storedProduce = 0;
            }
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
    }
}
