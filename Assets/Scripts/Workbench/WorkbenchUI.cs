using System;
using Brainrot;
using Helpers;
using Managers;
using TMPro;
using UI;
using UnityEngine;

namespace Workbench
{
    /// <summary>
    /// Управляет элементами интерфейса, связанными со станком
    /// </summary>
    public class WorkbenchUI : InfoUI
    {
        [SerializeField] private Workbench workbenchController;
        [SerializeField] private TextMeshProUGUI produceCounter;

        [SerializeField] private GameObject brainrotInfoGroup;
        
        [SerializeField] private TextMeshProUGUI brainrotNameText;
        [SerializeField] private TextMeshProUGUI brainrotTierText;
        [SerializeField] private TextMeshProUGUI produceRateText;
        [SerializeField] private TextMeshProUGUI lifeTimeText;

        private UIManager _uiManager;
        
        protected override void Awake()
        {
            base.Awake();
            
            _uiManager = UIManager.Instance;
        }
        
        protected override void Start()
        {
            base.Start();
            
            UpdateProduceCounter(workbenchController.storedProduce);
            if (workbenchController.GetInsertedBrainrot() is null)
                DisableProductionInfo();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            workbenchController.OnProduceUpdate += UpdateProduceCounter;
            workbenchController.OnBrainrotLifeTimeUpdate += UpdateLifeTime;
            workbenchController.OnProduceRateUpdate += UpdateProduceRate;
            workbenchController.OnBrainrotInsertion += UpdateBrainrotInfo;
            workbenchController.OnBrainrotDeath += DisableProductionInfo;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            workbenchController.OnProduceUpdate -= UpdateProduceCounter;
            workbenchController.OnBrainrotLifeTimeUpdate -= UpdateLifeTime;
            workbenchController.OnProduceRateUpdate -= UpdateProduceRate;
            workbenchController.OnBrainrotInsertion -= UpdateBrainrotInfo;
            workbenchController.OnBrainrotDeath -= DisableProductionInfo;
        }
        
        /// <summary>
        /// Обновляет счётчик хранимого в станке ресурса 
        /// </summary>
        private void UpdateProduceCounter(float amount)
        {
            var data = ValueShortener.CountShortener((long) Math.Round(amount));
            produceCounter.SetText(data.formatTemplate, data.value);
        }

        /// <summary>
        /// Обновляет счётчик времени жизни брейнрота, находящегося в станке
        /// </summary>
        /// <param name="lifeTime">Время жизни брейнрота</param>
        private void UpdateLifeTime(float lifeTime)
        {
            lifeTimeText.SetText(ValueShortener.TimeShortener(lifeTime));
        }

        /// <summary>
        /// Обновляет скорость выработки ресурса станком
        /// </summary>
        /// <param name="produceRate">Скорость выработки ресурса</param>
        private void UpdateProduceRate(float produceRate)
        {
            var data = ValueShortener.CountShortener(
                (long) Math.Round(produceRate), 
                postfix: " СК/сек.");
            produceRateText.SetText(data.formatTemplate, data.value);
        }

        /// <summary>
        /// Обновляет информацию о брейнроте, находящемся в станке
        /// </summary>
        /// <param name="brainrot">Брейнрот, находящийся в станке</param>
        private void UpdateBrainrotInfo(BrainrotObject brainrot)
        {
            var data = NameHelper.ParseBrainrotName(brainrot.GetBrainrotInfo().type);
            brainrotNameText.SetText(data);
            
            data = NameHelper.ParseRarity(brainrot.rarity);
            brainrotTierText.SetText(data);
            brainrotTierText.color = _uiManager.GetColors()[brainrot.rarity];
            
            EnableProductionInfo();
        }
        
        /// <summary>
        /// Отключает элементы интерфейса, отображающие информацию о производстве в станке
        /// </summary>
        private void DisableProductionInfo() => brainrotInfoGroup.SetActive(false);
        
        /// <summary>
        /// Включает элементы интерфейса, отображающие информацию о производстве в станке
        /// </summary>
        private void EnableProductionInfo() => brainrotInfoGroup.SetActive(true);
    }
}
