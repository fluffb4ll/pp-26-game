using System;
using System.Collections.Generic;
using Player;
using UnityEngine;

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

        public static GameManager Instance { get; private set; }
        
        public GameState currentState;
        public Transform playerTransform;
        public PlayerController playerController;
        public List<GameObject> spawnableBrainrots;
        private Action<GameState> _onGameStateStart;
        private Action<GameState> _onGameStateEnd;
        private readonly HashSet<SpawnManager> _spawnManagers = new();
        private int _combatSpawnHealthBonus;
        private int _killHealthBonus;

        void Awake()
        {
            Instance = this;
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

        public void RegisterSpawnManager(SpawnManager spawnManager)
        {
            _spawnManagers.Add(spawnManager);
        }

        public void UnregisterSpawnManager(SpawnManager spawnManager)
        {
            _spawnManagers.Remove(spawnManager);
        }

        public void ClearEnemiesOnMap()
        {
            foreach (var spawnManager in _spawnManagers)
                spawnManager.ClearSpawnedEnemies();
        }
    }
}
