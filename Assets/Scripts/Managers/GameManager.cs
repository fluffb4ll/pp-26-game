using System;
using UnityEngine;

namespace Managers
{
    public enum GameState
    {
        Home,
        Combat,
        GameOver
    }
    
    public class GameManager : MonoBehaviour
    {
        public GameState currentState;
        private Action<GameState> _onGameStateFinish;
        private Action<GameState> _onGameStateStart;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
        
        public event Action<GameState> OnGameStateFinish
        {
            add => _onGameStateFinish += value;
            remove => _onGameStateFinish -= value;
        }
        
        public event Action<GameState> OnGameStateStart
        {
            add => _onGameStateStart += value;
            remove => _onGameStateStart -= value;
        }

        public void ChangeGameState(GameState newState)
        {
            _onGameStateFinish?.Invoke(currentState);
            currentState = newState;
            _onGameStateStart?.Invoke(newState);
        }
    }
}
