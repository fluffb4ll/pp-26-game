using System;
using System.Reflection;
using YG;

namespace SDK
{
    /// <summary>
    /// Логика выдачи наград за ежедневный вход
    /// </summary>
    public class DailyRewards
    {
        private static bool _serverTimeMethodCached;
        private static MethodInfo _serverTimeMethod;

        public static long GetServerTime
        {
            get
            {
                var serverTimeMethod = GetServerTimeMethod();
                return serverTimeMethod is null
                    ? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    : Convert.ToInt64(serverTimeMethod.Invoke(null, null));
            }
        }
        
        /// <summary>
        /// Выдаёт награду за вход, если прошли сутки с момента получения последней награды
        /// </summary>
        public static void GiveDailyReward()
        {
            var serverTime = GetServerTime;

            if (serverTime - YG2.saves.lastDailyRewardTime < 24 * 60 * 60 * 1000)
                return;
            
            YG2.saves.coins++;  // плейсхолдер - в дальнейшем стоит поменять (например, на "календарь" наград)
            YG2.saves.lastDailyRewardTime = serverTime;
            YG2.SaveProgress();
        }

        private static MethodInfo GetServerTimeMethod()
        {
            if (_serverTimeMethodCached)
                return _serverTimeMethod;

            _serverTimeMethodCached = true;
            _serverTimeMethod = typeof(YG2).GetMethod("ServerTime", BindingFlags.Public | BindingFlags.Static);
            return _serverTimeMethod;
        }
    }
}
