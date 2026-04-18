using System;
using System.Collections.Generic;
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
        public GameState currentState;
        public List<GameObject> spawnableBrainrots;
        private Action<GameState> _onGameStateChange;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
        
        /// <summary>
        /// Событие, вызываемое в момент смены старого геймстейта на новый
        /// </summary>
        public event Action<GameState> OnGameStateChange
        {
            add => _onGameStateChange += value;
            remove => _onGameStateChange -= value;
        }
        
        /// <summary>
        /// Изменяет текущий геймстейт на указанный
        /// </summary>
        /// <param name="newState">Новый геймстейт</param>
        public void ChangeGameState(GameState newState)
        {
            currentState = newState;
            _onGameStateChange?.Invoke(newState);
        }
    }
}
