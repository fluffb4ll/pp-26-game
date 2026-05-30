using System;
using Helpers;
using Registries;
using SDK;
using Structs;
using UnityEngine;
using YG;

namespace Managers
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }
        
        [SerializeField] private float savingRate = 30f;
        [SerializeField] private const float SaveCooldownThreshold = 20f;
        
        private float _savingTimer;
        
        private EntityRegistry _entityRegistry;
        private GameManager _gameManager;
        
        private Action _onCoinsChanged;
        private Action _onSavesLoaded;

        private void Awake()
        {
            if (Instance is not null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            
            _entityRegistry = EntityRegistry.Instance;
            _gameManager = GameManager.Instance;
        }
        
        private void Start()
        {
            
        }
        
        private void Update()
        {
            HandleSavingData();
        }

        private void OnEnable()
        {
            YG2.onGetSDKData += LoadGameData;
            _entityRegistry.OnWorkbenchAdded += SaveNewWorkbench;
        }

        private void OnDisable()
        {
            YG2.onGetSDKData -= LoadGameData;
            _entityRegistry.OnWorkbenchAdded -= SaveNewWorkbench;
        }

        private void OnApplicationQuit()
        {
            TrySavingData();
        }

        private void OnApplicationFocus(bool focusStatus)
        {
            if (!focusStatus)
                SaveData();
        }
        
        public event Action OnSavesLoaded
        {
            add => _onSavesLoaded += value;
            remove => _onSavesLoaded -= value;
        }
        
        public event Action OnCoinsChanged
        {
            add => _onCoinsChanged += value;
            remove => _onCoinsChanged -= value;
        }

        private void LoadGameData()
        {
            LoadWorkbenches();
            CalculateOfflineWork();
            HandleSavingData();
            _entityRegistry.FinishIgnoringSavedObjects();
        }
        
        private void HandleSavingData()
        {
            if (_savingTimer > 0)
            {
                _savingTimer -= Time.deltaTime;
                return;
            }
            
            SaveData();
        }
        
        /// <summary>
        /// Сохраняет данные игрока
        /// </summary>
        public void SaveData()
        {
            SetNewLastSaveTime();
            YG2.SaveProgress();
            _savingTimer = savingRate;
        }

        /// <summary>
        /// Обёртка вокруг <see cref="SaveData"/>, защищающая от спама сохранениями.
        /// </summary>
        public void TrySavingData()
        {
            if (_savingTimer >= savingRate * (1 - SaveCooldownThreshold / 100))
                return;

            SaveData();
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
        
        private void SetNewLastSaveTime() => YG2.saves.lastSaveTime = DailyRewards.GetServerTime;

        /// <summary>
        /// Вызывает методы <see cref="Workbench.Workbench.CalculateOfflineWork"/> у всех загруженных станков
        /// </summary>
        private void CalculateOfflineWork()
        {
            var timedelta = DailyRewards.GetServerTime - YG2.saves.lastSaveTime;
            foreach (var workbench in _entityRegistry.GetWorkbenches().Values)
                workbench.CalculateOfflineWork(timedelta);
        }
        
        public int GetNextEnemyHealthBonus()
        {
            return YG2.saves.enemyHealthBonus += _gameManager.spawnHealthBonusStep;
        }

        public void LowerCombatSpawnHealthBonus()
        {
            var finalValue = YG2.saves.enemyHealthBonus - _gameManager.spawnHealthBonusStep * 5;
            var enemyInitialMaxHealth = _gameManager.enemyInitialMaxHealth;
            YG2.saves.enemyHealthBonus = finalValue < enemyInitialMaxHealth ? enemyInitialMaxHealth : finalValue;
        }

        /// <summary>
        /// Сохраняет купленный станок в облако
        /// </summary>
        /// <param name="workbench">Купленный станок</param>
        private void SaveNewWorkbench(Workbench.Workbench workbench)
        {
            var brainrot = workbench.GetInsertedBrainrot();
            var position = new Position(workbench.transform.position);
            var rotation = new Rotation(workbench.transform.rotation);
            var save = new WorkbenchSave(workbench.baseProduce, workbench.produceStoreCap, workbench.storedProduce,
                position, rotation, false);
            if (brainrot is not null)
            {
                var type = brainrot.GetBrainrotInfo().type;
                save.hasBrainrot = true;
                save.type = type;
                save.rarity = brainrot.rarity;
                save.brainrotLifeTime = brainrot.lifetime;
                save.brainrotProduce = brainrot.produce;
            }

            YG2.saves.workbenches.Add(workbench.GetEntityIdHash(), save);
            SaveData();
            
            workbench.OnBrainrotDeath += UpdateWorkbenchDataOnBrainrotDeath;
            workbench.OnBrainrotInsertion += UpdateWorkbenchDataOnBrainrotInsertion;
            workbench.OnCollectCoins += UpdateWorkbenchDataOnCoinCollection;
        }

        public void UpdateWorkbenchData(Workbench.Workbench workbench)
        {
            var workbenchSave = YG2.saves.workbenches[workbench.GetEntityIdHash()];
            workbenchSave.storedProduce = workbench.storedProduce;
            workbenchSave.baseProduce = workbench.baseProduce;
            workbenchSave.produceStoreCap = workbench.produceStoreCap;

            if (workbench.GetInsertedBrainrot() is not null) 
                if (!workbenchSave.hasBrainrot)
                {
                    workbenchSave.hasBrainrot = true;
                    var brainrot = workbench.GetInsertedBrainrot();
                    workbenchSave.type = brainrot.GetBrainrotInfo().type;
                    workbenchSave.rarity = brainrot.rarity;
                    workbenchSave.brainrotLifeTime = brainrot.lifetime;
                    workbenchSave.brainrotProduce = brainrot.produce;
                }
                else
                    workbenchSave.brainrotLifeTime = workbench.GetInsertedBrainrot().lifetime;
            
            YG2.saves.workbenches[workbench.GetEntityIdHash()] = workbenchSave;
            SaveData();
        }

        private void UpdateWorkbenchDataOnBrainrotDeath(Workbench.Workbench workbench)
        {
            var workbenchSave = YG2.saves.workbenches[workbench.GetEntityIdHash()];
            
            workbenchSave.storedProduce = workbench.storedProduce;
            workbenchSave.baseProduce = workbench.baseProduce;
            workbenchSave.produceStoreCap = workbench.produceStoreCap;
            workbenchSave.hasBrainrot = false;
            
            YG2.saves.workbenches[workbench.GetEntityIdHash()] = workbenchSave;
            SaveData();
        }

        private void UpdateWorkbenchDataOnBrainrotInsertion(Workbench.Workbench workbench)
        {
            var workbenchSave = YG2.saves.workbenches[workbench.GetEntityIdHash()];
            
            workbenchSave.storedProduce = workbench.storedProduce;
            workbenchSave.baseProduce = workbench.baseProduce;
            workbenchSave.produceStoreCap = workbench.produceStoreCap;
            
            workbenchSave.hasBrainrot = true;
            var brainrot = workbench.GetInsertedBrainrot();
            workbenchSave.type = brainrot.GetBrainrotInfo().type;
            workbenchSave.rarity = brainrot.rarity;
            workbenchSave.brainrotLifeTime = brainrot.lifetime;
            workbenchSave.brainrotProduce = brainrot.produce;
            
            YG2.saves.workbenches[workbench.GetEntityIdHash()] = workbenchSave;
            SaveData();
        }

        private void UpdateWorkbenchDataOnCoinCollection(Workbench.Workbench workbench)
        {
            var workbenchSave = YG2.saves.workbenches[workbench.GetEntityIdHash()];
            
            workbenchSave.storedProduce = workbench.storedProduce;
            workbenchSave.baseProduce = workbench.baseProduce;
            workbenchSave.produceStoreCap = workbench.produceStoreCap;
            
            YG2.saves.workbenches[workbench.GetEntityIdHash()] = workbenchSave;
            SaveData();
        }

        private void LoadWorkbenches()
        {
            if (YG2.saves.workbenches is null || YG2.saves.workbenches.Count == 0)
                return;
            
            var workbenchPrefab = _entityRegistry.GetWorkbenchPrefab();
            foreach (var savedWorkbench in YG2.saves.workbenches.Values)
            {
                var tPos = savedWorkbench.position;
                var tRot = savedWorkbench.rotation;
                var position = new Vector3(tPos.x, tPos.y, tPos.z);
                var rotation = new Quaternion(tRot.x, tRot.y, tRot.z, tRot.w);
                
                var workbench = Instantiate(workbenchPrefab, position, rotation)
                    .GetComponent<Workbench.Workbench>();
                workbench.LoadSavedData(savedWorkbench.baseProduce, savedWorkbench.produceStoreCap, savedWorkbench.storedProduce);
                
                if (savedWorkbench.hasBrainrot)
                    workbench.LoadBrainrotData(
                        savedWorkbench.brainrotProduce, savedWorkbench.brainrotLifeTime, 
                        savedWorkbench.type, savedWorkbench.rarity);
                
                workbench.OnBrainrotDeath += UpdateWorkbenchDataOnBrainrotDeath;
                workbench.OnBrainrotInsertion += UpdateWorkbenchDataOnBrainrotInsertion;
                workbench.OnCollectCoins += UpdateWorkbenchDataOnCoinCollection;
            }
        }
    }
}
