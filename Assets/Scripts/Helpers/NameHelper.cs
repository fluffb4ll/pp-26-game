using Brainrot;

namespace Helpers
{
    /// <summary>
    /// Парсит названия различных предметов.
    /// </summary>
    public static class NameHelper
    {
        /// <summary>
        /// Парсит название редкости брейнрота. На данный момент выдаёт только русские варианты названий.
        /// </summary>
        /// <param name="rarity">Редкость, которую нужно распарсить</param>
        /// <returns>Название редкости брейнрота</returns>
        public static string ParseRarity(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Common => "Обычный",
                Rarity.Uncommon => "Необычный",
                Rarity.Rare => "Редкий",
                Rarity.Epic => "Эпический",
                Rarity.Legendary => "Легендарный",
                _ => "???"
            };
        }

        /// <summary>
        /// Парсит название брейнрота, основываясь на его типе. На данный момент выдаёт только русские варианты названий.
        /// </summary>
        /// <param name="brainrotType">Тип брейнрота</param>
        /// <returns>Название брейнрота</returns>
        public static string ParseBrainrotName(BrainrotType brainrotType)
        {
            return brainrotType switch
            {
                BrainrotType.Sigma => "Сигма",
                BrainrotType.SkibidiToilet => "Скибиди Туалет",
                BrainrotType.TTTSahur => "Сахур",
                _ => "???"
            };
        }
    }
}