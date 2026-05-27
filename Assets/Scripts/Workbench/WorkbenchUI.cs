using System;
using Helpers;
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
        
        protected override void Start()
        {
            base.Start();
            UpdateProduceCounter(workbenchController.storedProduce);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            workbenchController.OnProduceUpdate += UpdateProduceCounter;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            workbenchController.OnProduceUpdate -= UpdateProduceCounter;
        }
        
        /// <summary>
        /// Обновляет счётчик хранимого в станке ресурса 
        /// </summary>
        private void UpdateProduceCounter(float amount)
        {
            var data = ValueShortener.CountShortener((long) Math.Round(amount));
            produceCounter.SetText(data.formatTemplate, data.value);
        }
    }
}
