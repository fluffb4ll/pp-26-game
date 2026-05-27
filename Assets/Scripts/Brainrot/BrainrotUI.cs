using System;
using Helpers;
using Managers;
using Mono.Cecil;
using TMPro;
using UI;
using UnityEngine;

namespace Brainrot
{
    /// <summary>
    /// Управляет элементами интерфейса, связанными с брейнротом
    /// </summary>
    public class BrainrotUI : InfoUI
    {
        [SerializeField] private TextMeshProUGUI tierText;
        [SerializeField] private TextMeshProUGUI produceRateText;
        [SerializeField] private TextMeshProUGUI lifeTimeText;
        
        [SerializeField] private BrainrotObject brainrotObject;
        
        private UIManager _uiManager;
        private bool _isInitialized;

        protected override void Awake()
        {
            base.Awake();
            _uiManager = UIManager.Instance;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            brainrotObject.OnBrainrotRoll += UpdateInfoCanvas;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            brainrotObject.OnBrainrotRoll -= UpdateInfoCanvas;
        }
        
        /// <summary>
        /// Заполняет <c>InfoCanvas</c> информацией о брейнроте
        /// </summary>
        private void UpdateInfoCanvas()
        {
            lifeTimeText.SetText(ValueShortener.TimeShortener(brainrotObject.lifetime));
            if (_isInitialized) return;
            var data = ValueShortener.CountShortener(
                (long)Math.Round(brainrotObject.produce), 
                postfix: " СК/сек.");
            produceRateText.SetText(data.formatTemplate, data.value);
            
            tierText.color = _uiManager.GetColors()[brainrotObject.rarity];
            tierText.SetText(NameHelper.ParseRarity(brainrotObject.rarity));
            
            _isInitialized = true;
        }
    }
}
