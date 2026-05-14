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
        
        private string _paramName;
        private AudioManager _audioManager;

        void Start()
        {
            _audioManager = AudioManager.Instance;
            _paramName = _audioManager.audioMixerGroups[audioMixerGroupIndex];
        }

        public void OnValueChanged(Slider slider)
        {
            _audioManager.SetVolume(_paramName, slider.value);
        }
    }
}
