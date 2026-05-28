using System;
using System.Collections.Generic;
using Player;
using UnityEngine;
using YG;

namespace Managers
{
    public enum GameState
    {
        Home,
        Combat,
        GameOver
    }
    
    /// <summary>
    /// Объект, управляющий общей игровой логикой
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        [SerializeField] private int spawnHealthBonusStep = 5;
        [SerializeField] private int killHealthBonusStep = 10;
        [SerializeField] private float savingRate;
        [SerializeField] private int savingThreshold;

        [SerializeField] private List<SpawnManager> spawners;
            
        public GameState currentState;
        public Transform playerTransform;
        public PlayerController playerController;
        public List<GameObject> spawnableBrainrots;
        
        private float _savingTimer;
        private Action<GameState> _onGameStateStart;
        private Action<GameState> _onGameStateEnd;
        private int _combatSpawnHealthBonus;
        private int _killHealthBonus;

        private Action _onCoinsChanged;
        
        void Awake()
        {
            Instance = this;
            _savingTimer = savingRate;
        }

        void Update()
        {
            HandleSavingData();
        }
        
        /// <summary>
        /// Событие, вызываемое в момент смены старого геймстейта на новый
        /// </summary>
        public event Action<GameState> OnGameStateStart
        {
            add => _onGameStateStart += value;
            remove => _onGameStateStart -= value;
        }
        
        /// <summary>
        /// Событие, вызываемое в момент смены старого геймстейта на новый
        /// </summary>
        public event Action<GameState> OnGameStateEnd
        {
            add => _onGameStateEnd += value;
            remove => _onGameStateEnd -= value;
        }
        
        public event Action OnCoinsChanged
        {
            add => _onCoinsChanged += value;
            remove => _onCoinsChanged -= value;
        }
        
        /// <summary>
        /// Изменяет текущий геймстейт на указанный
        /// </summary>
        /// <param name="newState">Новый геймстейт</param>
        public void ChangeGameState(GameState newState)
        {
            _onGameStateEnd?.Invoke(currentState);
            currentState = newState;
            _onGameStateStart?.Invoke(newState);
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

        public int GetNextEnemyHealthBonus()
        {
            _combatSpawnHealthBonus += spawnHealthBonusStep;
            return _combatSpawnHealthBonus + _killHealthBonus;
        }

        public void RegisterEnemyKill()
        {
            _killHealthBonus += killHealthBonusStep;
        }

        public void ResetCombatSpawnHealthBonus()
        {
            _combatSpawnHealthBonus = 0;
        }

        public int GetCurrentQuestID() => YG2.saves.currentQuest;
        
        public void SetCurrentQuestID(int currentQuestID) => YG2.saves.currentQuest = currentQuestID;
        
        public int IncrementCurrentQuestID() => ++YG2.saves.currentQuest;
        
        public List<SpawnManager> GetSpawners() => spawners;
    }
}
