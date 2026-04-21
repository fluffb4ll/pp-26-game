using System.Collections.Generic;
using Enemy;
using UnityEngine;

namespace Managers
{
    /// <summary>
    /// Управляет спавнерами в боевой зоне
    /// </summary>
    public class SpawnManager : MonoBehaviour
    {
        [SerializeField] private GameObject spawnablePrefab;
        [SerializeField] private int enemyLimit;
        [SerializeField] private float spawnRate;
        
        private GameManager _gameManager;
        private HashSet<GameObject> _spawnedEnemies;
        private float _spawnTimer;
        private bool _isSpawning;

        void Awake()
        {
            _spawnTimer = spawnRate;
            _gameManager = GameManager.Instance;
            _spawnedEnemies = new HashSet<GameObject>();
        }

        void OnEnable()
        {
            _gameManager.OnGameStateEnd += OnCombatEnd;
            _gameManager.OnGameStateStart += OnCombatStart;
        }

        void OnDisable()
        {
            _gameManager.OnGameStateEnd -= OnCombatEnd;
            _gameManager.OnGameStateStart -= OnCombatStart;
        }

        // Update is called once per frame
        void Update()
        {
            HandleSpawning();
        }

        private void HandleSpawning()
        {
            if (!_isSpawning || _spawnedEnemies.Count >= enemyLimit) 
                return;
            
            _spawnTimer -= Time.deltaTime;
            
            if (_spawnTimer > 0) 
                return;
            
            var spawnedEnemy = Instantiate(spawnablePrefab, transform.position, transform.rotation);
            spawnedEnemy.GetComponent<EnemyCombat>().spawnManager = this;
            _spawnedEnemies.Add(spawnedEnemy);
            _spawnTimer = spawnRate;
        }
        
        private void OnCombatStart(GameState newState)
        {
            if (newState == GameState.Combat)
                _isSpawning = true;
        }

        private void OnCombatEnd(GameState oldState)
        {
            if (oldState != GameState.Combat) 
                return;
            
            _isSpawning = false;

            foreach (var obj in _spawnedEnemies)
                Destroy(obj);
            
            _spawnTimer = spawnRate;
            _spawnedEnemies.Clear();
        }

        public void UnregisterEnemy(GameObject enemy)
        {
            _spawnedEnemies.Remove(enemy);
        }
    }
}
