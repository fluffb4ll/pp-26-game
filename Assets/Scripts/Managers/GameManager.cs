using System;
using System.Collections.Generic;
using Player;
using UnityEditor.Overlays;
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
        [SerializeField] private int spawnHealthBonusStep = 5;
        [SerializeField] private int killHealthBonusStep = 10;
        [SerializeField] private float savingRate;
        public static GameManager Instance { get; private set; }
        
        public GameState currentState;
        public Transform playerTransform;
        public PlayerController playerController;
        public List<GameObject> spawnableBrainrots;
        
        private float _savingTimer;
        private Action<GameState> _onGameStateStart;
        private Action<GameState> _onGameStateEnd;
        private int _combatSpawnHealthBonus;
        private int _killHealthBonus;

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

        private void HandleSavingData()
        {
            if (_savingTimer > 0)
                _savingTimer -= Time.deltaTime;
            else
                SaveData();
        }

        public void SaveData()
        {
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
    }
}
