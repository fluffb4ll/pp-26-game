using System;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Вспомогательный класс для слайдеров, управляющих громкостью звука
    /// </summary>
    public class VolumeSlider : MonoBehaviour
    {
        public int audioMixerGroupIndex;
        [SerializeField] private Slider slider;
        private string _paramName;
        private AudioManager _audioManager;

        private void Awake()
        {
            _audioManager = AudioManager.Instance;
        }

        private void OnEnable()
        {
            _paramName ??= _audioManager.audioMixerGroups[audioMixerGroupIndex];
            LoadSavedSliderValue();
        }
        
        public void OnValueChanged()
        {
            _audioManager.SetVolume(_paramName, slider.value);
        }
        
        /// <summary>
        /// Выставляет слайдер в сохранённое ранее положение
        /// </summary>
        private void LoadSavedSliderValue()
        {
            var value = AudioManager.GetSavedVolumeValues(_paramName);
            slider.value = value;
        }
    }
}
