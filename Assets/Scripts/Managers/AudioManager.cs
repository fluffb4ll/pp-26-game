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

        public List<AudioClip> music;
        
        //private const float VolumeMinValue = 0.0001f;

        private void Awake()
        {
            if (!ReferenceEquals(Instance, null) && !ReferenceEquals(Instance, this))
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
        }

        private void Start()
        {
            SetVolume("MusicVol", PlayerPrefs.GetFloat("MusicVol", 1f));
        }
        
        /// <summary>
        /// Изменяет значение громкости и сохраняет его в <see cref="PlayerPrefs"/>
        /// </summary>
        /// <param name="groupName">Название группы <see cref="AudioMixerGroup"/></param>
        /// <param name="value">Устанавливаемое значение</param>
        public void SetVolume(string groupName, float value)
        {
            var clampedValue = Mathf.Clamp01(value);
            float volume;
            
            if (clampedValue <= 0f)
                volume = -80f;
            else        
                volume = Mathf.Lerp(-80f, 0f, Mathf.Log10(clampedValue) + 1f);
            
            audioMixer.SetFloat(groupName, volume);
            PlayerPrefs.SetFloat(groupName, clampedValue);
            PlayerPrefs.Save();
        }
    }
}
