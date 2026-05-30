using System;
using UnityEngine;
using YG;

namespace Managers
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }
        
        [SerializeField] private float savingRate;
        [SerializeField] private int savingThreshold;
        
        private float _savingTimer;
        
        private Action _onCoinsChanged;

        private void Awake()
        {
            if (Instance is not null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            
            _savingTimer = savingRate;
        }
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }
        
        private void Update()
        {
            HandleSavingData();
        }
        
        public event Action OnCoinsChanged
        {
            add => _onCoinsChanged += value;
            remove => _onCoinsChanged -= value;
        }
        
        private void HandleSavingData()
        {
            if (_savingTimer > 0)
                _savingTimer -= Time.deltaTime;
            else
                SaveData();
        }
        
        /// <summary>
        /// Сохраняет данные игрока
        /// </summary>
        public void SaveData()
        {
            // TODO: проверить на соответствие условиям частоты сохранений в ЯИ
            if (_savingTimer >= savingRate * (1 - Mathf.Clamp(savingThreshold, 0, 100) / 100f))
                return;
            
            YG2.SaveProgress();
            _savingTimer = savingRate;
        }
        
        /// <summary>
        /// Возвращает количество монет у игрока
        /// </summary>
        public long GetCoinsAmount()
        {
            return YG2.saves.coins;
        }
        
        /// <summary>
        /// Изменяет число монет у игрока.
        /// Обновлять число монет стоит ТОЛЬКО через этот метод.
        /// </summary>
        /// <param name="delta">Изменение количества монет</param>
        public void ChangeCoinsAmount(long delta)
        {
            if (YG2.saves.coins + delta < 0)
                return;
            
            YG2.saves.coins += delta;
            _onCoinsChanged?.Invoke();
        }
        
        public int GetCurrentQuestID() => YG2.saves.currentQuest;
        
        public void SetCurrentQuestID(int currentQuestID) => YG2.saves.currentQuest = currentQuestID;
        
        public int IncrementCurrentQuestID() => ++YG2.saves.currentQuest;
    }
}
