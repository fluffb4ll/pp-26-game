using YG;

namespace SDK
{
    /// <summary>
    /// Логика выдачи наград за ежедневный вход
    /// </summary>
    public class DailyRewards
    {
        public static long GetServerTime => YG2.ServerTime();
        
        /// <summary>
        /// Выдаёт награду за вход, если прошли сутки с момента получения последней награды
        /// </summary>
        public static void GiveDailyReward()
        {
            if (GetServerTime - YG2.saves.lastDailyRewardTime < 24 * 60 * 60 * 1000)
                return;
            
            YG2.saves.coins++;  // плейсхолдер - в дальнейшем стоит поменять (например, на "календарь" наград)
            YG2.saves.lastDailyRewardTime = GetServerTime;
        }
    }
}
