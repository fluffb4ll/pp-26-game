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
        public List<string> audioMixerGroups;
        
        private const float MaxDb = 0f;
        private const float MinDb = -80f;

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
            LoadSavedVolumeValues();
        }
        
        /// <summary>
        /// Изменяет значение громкости и сохраняет его в <see cref="PlayerPrefs"/>
        /// </summary>
        /// <param name="groupName">Название группы <see cref="AudioMixerGroup"/></param>
        /// <param name="value">Устанавливаемое значение</param>
        public void SetVolume(string groupName, float value)
        {
            var clampedValue = Mathf.Clamp01(value);
            var volume = clampedValue <= 0f ? MinDb : Mathf.Lerp(MinDb, MaxDb, Mathf.Log10(clampedValue) + 1f);
            
            audioMixer.SetFloat(groupName, volume);
            PlayerPrefs.SetFloat(groupName, clampedValue);
        }

        /// <summary>
        /// Передаёт сохранённое значение громкости указанной <c>AudioMixerGroup</c>.
        /// </summary>
        /// <param name="groupName">Название <c>AudioMixerGroup</c>, значение которой нужно получить</param>
        public static float GetSavedVolumeValues(string groupName) => PlayerPrefs.GetFloat(groupName, 1f);

        /// <summary>
        /// Выставляет громкость, равную сохранённым значениям
        /// </summary>
        private void LoadSavedVolumeValues()
        {
            foreach (var group in audioMixerGroups)
                SetVolume(group, PlayerPrefs.GetFloat(group, 1f));
        }
    }
}
