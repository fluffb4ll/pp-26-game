using System;
using System.Threading.Tasks;
using YG;
using YG.Utils.LB;

namespace SDK
{
    /// <summary>
    /// Содержит методы для управления лидербордами
    /// </summary>
    public class Leaderboards
    {
        /// <summary>
        /// Перезаписывает запись в лидерборде <c>tableName</c>.
        /// Использовать, если лидерборд отображает какой-то изменяющийся показатель
        /// (например, если таблица показывает, у какого пользователя на данный момент находится
        /// максимальное количество ресурса X).
        /// В противном случае, используйте <see cref="SetLeaderboardRecord"/>
        /// </summary>
        /// <param name="tableName">Техническое название таблицы (берём из консоли разработчика на сервисе)</param>
        /// <param name="newValue">Записываемое значение</param>
        /// <remarks>ВАЖНО: запрос можно отправлять не чаще, чем раз в секунду.</remarks>
        public static void SetLeaderboardEntry(string tableName, int newValue)
        {
            YG2.SetLeaderboard(tableName, newValue);
        }

        /// <summary>
        /// Обновляет рекорд в лидерборде <c>tableName</c>.
        /// Перед обновлением рекорда сверяет <c>newRecord</c> с существующим в таблице значением
        /// и обновляет его, только если <c>newRecord</c> больше существующего.
        /// Если вам нужно, чтобы запись обновлялась в любом случае - используйте <see cref="SetLeaderboardEntry"/>
        /// </summary>
        /// <param name="tableName">Техническое название таблицы (берём из консоли разработчика на сервисе)</param>
        /// <param name="newRecord">Новый рекорд</param>
        /// <remarks>ВАЖНО: запрос можно отправлять не чаще, чем раз в секунду.</remarks>
        public static async void SetLeaderboardRecord(string tableName, int newRecord)
        {
            LBCurrentPlayerData data = await GetPlayerLeaderboardEntryAsync(tableName);
        
            if (data.score < newRecord)
                SetLeaderboardEntry(tableName, newRecord);
        }
    
        /// <summary>
        /// Асинхронно получает данные текущего игрока из лидерборда <c>tableName</c>.
        /// </summary>
        /// <param name="tableName">Техническое название таблицы (берём из консоли разработчика на сервисе)</param>
        /// <param name="timeoutMs">Таймаут: время (в мс), после которого ожидание ответа от сервера прекратится</param>
        /// <returns>Данные текущего игрока, внесённые в лидерборд: <see cref="YG.Utils.LB.LBCurrentPlayerData"/></returns>
        /// <exception cref="TimeoutException">Выбрасывается, если сервер не ответил на запрос за время <c>timeoutMs</c></exception>
        private static async Task<LBCurrentPlayerData> GetPlayerLeaderboardEntryAsync(string tableName, int timeoutMs = 5000)
        {
            var tcs = new TaskCompletionSource<LBCurrentPlayerData>();

            void OnLeaderboardRecieved(LBData data)
            {
                if (data.technoName != tableName)
                    return;
            
                YG2.onGetLeaderboard -= OnLeaderboardRecieved;
            
                tcs.TrySetResult(data.currentPlayer);
            }
        
            YG2.onGetLeaderboard += OnLeaderboardRecieved;
            YG2.GetLeaderboard(tableName);
        
            Task timeoutTask = Task.Delay(timeoutMs);
        
            if (await Task.WhenAny(tcs.Task, timeoutTask) == timeoutTask) 
            {
                YG2.onGetLeaderboard -= OnLeaderboardRecieved;
                throw new TimeoutException($"Превышено время ожидания данных лидерборда {tableName}");
            }
        
            return await tcs.Task;
        }
    }
}
