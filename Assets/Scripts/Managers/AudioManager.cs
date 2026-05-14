using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using PlayerPrefs = RedefineYG.PlayerPrefs;

namespace Managers
{
    /// <summary>
    /// Управление звуком
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        public AudioMixer audioMixer;
        public List<String> audioMixerGroups;

        private const float VolumeMinValue = 0.0001f;

        private void Awake()
        {
            if (!ReferenceEquals(Instance, null) && !ReferenceEquals(Instance, this))
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
        }
        
        /// <summary>
        /// Изменяет значение громкости и сохраняет его в <see cref="PlayerPrefs"/>
        /// </summary>
        /// <param name="groupName">Название группы <see cref="AudioMixerGroup"/></param>
        /// <param name="value">Устанавливаемое значение</param>
        public void SetVolume(string groupName, float value)
        {
            var volume = Mathf.Log10(Mathf.Clamp(value, VolumeMinValue, 1f)) * 20;
            audioMixer.SetFloat(groupName, volume);
            PlayerPrefs.SetFloat(groupName, volume);
        }
    }
}
