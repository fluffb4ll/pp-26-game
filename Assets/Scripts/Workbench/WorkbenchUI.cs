using Camera;
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
            UpdateProduceCounter();
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
        private void UpdateProduceCounter()
        {
            var amount = Mathf.RoundToInt(workbenchController.storedProduce);
            
            var newValue = amount switch
            {
                > 1000000000 => (amount / 1000000000.0).ToString("F1") + "B",
                > 1000000 => (amount / 1000000.0).ToString("F1") + "M",
                > 10000 => (amount / 1000.0).ToString("F1") + "K",
                _ => amount.ToString()
            };

            produceCounter.text = newValue;
        }
    }
}
