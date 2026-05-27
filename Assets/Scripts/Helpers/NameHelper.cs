using Brainrot;

namespace Helpers
{
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