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
        [SerializeField] private TextMeshProUGUI produceText;
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
        
        private void UpdateInfoCanvas()
        {
            lifeTimeText.SetText(ValueShortener.TimeShortener(brainrotObject.lifetime));
            if (_isInitialized) return;
            var data = ValueShortener.CountShortener(
                (long)Math.Round(brainrotObject.produce), 
                postfix: " СК/сек");
            produceText.SetText(data.formatTemplate, data.value);
            
            tierText.color = _uiManager.GetColors()[brainrotObject.rarity];
            var tierName = brainrotObject.rarity switch
            {
                Rarity.Common => "Обычный",
                Rarity.Uncommon => "Необычный",
                Rarity.Rare => "Редкий",
                Rarity.Epic => "Эпический",
                Rarity.Legendary => "Легендарный",
                _ => "???"
            };
            tierText.SetText(tierName);
            
            _isInitialized = true;
        }
    }
}
