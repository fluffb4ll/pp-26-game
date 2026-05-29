using System;
using System.Collections.Generic;
using Brainrot;
using Enemy;
using Helpers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
    /// <summary>
    /// Управляет спавнерами в боевой зоне
    /// </summary>
    public class SpawnManager : MonoBehaviour
    {
        [SerializeField] private string entityId;
        
        [SerializeField] private GameObject spawnablePrefab;
        [SerializeField] private int enemyLimit;
        [SerializeField] private float spawnRate;

        private int _entityIdHash;
        
        private GameManager _gameManager;
        private HashSet<GameObject> _spawnedEnemies;
        private List<GameObject> _spawnableBrainrots;
        private float _spawnTimer;
        private bool _isSpawning;

        private Action<GameObject> _onEnemyDeath;
        private Action _onBrainrotSpawn;
        
        void Awake()
        {
            _spawnTimer = spawnRate;
            _gameManager = GameManager.Instance;
            _spawnableBrainrots = _gameManager.spawnableBrainrots;
            _spawnedEnemies = new HashSet<GameObject>();
            
            _entityIdHash = EntityRegistry.Instance.AddSpawner(this);
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

        private void OnDestroy()
        {
            EntityRegistry.Instance?.RemoveSpawner(_entityIdHash);
        }
        
        public event Action<GameObject> OnEnemyDeath
        {
            add => _onEnemyDeath += value;
            remove => _onEnemyDeath -= value;
        }
        
        public event Action OnBrainrotSpawn
        {
            add => _onBrainrotSpawn += value;
            remove => _onBrainrotSpawn -= value;
        }

        private void HandleSpawning()
        {
            if (!_isSpawning || _spawnedEnemies.Count >= enemyLimit) 
                return;
            
            _spawnTimer -= Time.deltaTime;
            
            if (_spawnTimer > 0) 
                return;
            
            var spawnedEnemy = Instantiate(spawnablePrefab, transform.position, transform.rotation);
            var assignedBrainrot = _spawnableBrainrots[Random.Range(0, _spawnableBrainrots.Count)];
            spawnedEnemy
                .GetComponent<EnemyCombat>()
                .InitializeSpawn(
                    this, 
                    _gameManager.GetNextEnemyHealthBonus(), 
                    assignedBrainrot);
            
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
            ClearSpawnedEnemies();
            _spawnTimer = spawnRate;
        }

        public void UnregisterEnemy(GameObject enemy)
        {
            _spawnedEnemies.Remove(enemy);
            _onEnemyDeath?.Invoke(enemy);
            
            enemy.GetComponent<EnemyCombat>().GetSpawnableBrainrot().GetComponent<BrainrotObject>().OnInteract +=
                _onBrainrotSpawn;
        }

        private void ClearSpawnedEnemies()
        {
            foreach (var enemy in _spawnedEnemies)
                Destroy(enemy);

            _spawnedEnemies.Clear();
        }
    }
}
