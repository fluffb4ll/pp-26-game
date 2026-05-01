using System;
using System.Collections.Generic;
using UnityEngine;

namespace Brainrot
{
    /// <summary>
    /// Доступные в игре редкости
    /// </summary>
    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// Доступные в игре типы брейнротов
    /// </summary>
    public enum BrainrotType
    {
        SkibidiToilet,
        Sigma,
        TTTSahur
    }

    /// <summary>
    /// Конфигурация редкости - соотносим редкость с множителями времени жизни и выработки и шансом выпадения
    /// </summary>
    [Serializable]
    public struct RarityConfig
    {
        public Rarity rarity;
        public float lifetimeMult;
        public float produceMult;
        [Range(0, 100)] public int dropWeight;
    }

    /// <summary>
    /// Позволяет добавлять объявления брейнротов внутри инспектора - просто и безболезненно (надеюсь) :)
    /// </summary>
    [CreateAssetMenu(fileName = "BrainrotLib", menuName = "Scriptable Objects/BrainrotLib")]
    public class BrainrotLib : ScriptableObject
    {
        public BrainrotType type;
        public float baseProduce;
        public float baseLifetime;
    
        public List<RarityConfig> rarityPool;
    
        /// <summary>
        /// Возвращает случайно выбранную редкость на основе весов, представленных в <c>rarityPool</c>
        /// </summary>
        /// <returns>Выбранный объект <see cref="RarityConfig"/></returns>
        public RarityConfig GetRandomizedRarity()
        {
            var totalWeight = 0;
            foreach (var config in rarityPool)
                totalWeight += config.dropWeight;
        
            var random = UnityEngine.Random.Range(0, totalWeight);
            var currentSum = 0;

            foreach (var config in rarityPool)
            {
                currentSum += config.dropWeight;
            }
        
            // не возвращаю null на случай, если проебались и не хотим, чтобы всё пошло по пизде :)
            return rarityPool[0];
        }
    }
}
