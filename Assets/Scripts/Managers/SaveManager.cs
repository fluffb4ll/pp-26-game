using System;
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
        
        private Action _onCoinsChanged;

        private void Awake()
        {
            if (Instance is not null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            
            _entityRegistry = EntityRegistry.Instance;
        }
        
        private void Start()
        {
            LoadWorkbenches();
            CalculateOfflineWork();
            HandleSavingData();
        }
        
        private void Update()
        {
            HandleSavingData();
        }

        private void OnEnable()
        {
            _entityRegistry.OnWorkbenchAdded += SaveNewWorkbench;
        }

        private void OnDisable()
        {
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
        
        public event Action OnCoinsChanged
        {
            add => _onCoinsChanged += value;
            remove => _onCoinsChanged -= value;
        }
        
        private void HandleSavingData()
        {
            if (_savingTimer > 0)
            {
                _savingTimer -= Time.deltaTime;
                return;
            }
            
            SetNewLastSaveTime();
            SaveData();
        }
        
        /// <summary>
        /// Сохраняет данные игрока
        /// </summary>
        public void SaveData()
        {
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

        /// <summary>
        /// Сохраняет купленный станок в облако
        /// </summary>
        /// <param name="workbench">Купленный станок</param>
        private void SaveNewWorkbench(Workbench.Workbench workbench)
        {
            var save = new WorkbenchSave(workbench.storedProduce, workbench.GetInsertedBrainrot(),
                workbench.transform.position, workbench.transform.rotation);
            YG2.saves.workbenches.Add(save);
            
            SaveData();
        }

        private void LoadWorkbenches()
        {
            if (YG2.saves.workbenches.Count == 0)
                return;
            
            var workbenchPrefab = _entityRegistry.GetWorkbenchPrefab();
            foreach (var workbench in YG2.saves.workbenches)
            {
                Instantiate(workbenchPrefab, workbench.position, workbench.rotation);
            }
        }
    }
}
