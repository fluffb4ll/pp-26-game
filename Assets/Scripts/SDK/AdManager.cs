using Managers;
using UnityEngine;
using YG;

namespace SDK
{
    /// <summary>
    /// Содержит идентификаторы <c>rewardId</c>, используемые для выдачи наград за просмотр рекламы
    /// </summary>
    /// <remarks>Меняете константы - меняйте и кейсы в <see cref="AdManager.ShowRewardAdv()"/></remarks>
    public static class AdRewards
    {
        public const string Coins = "coins";
    }

    /// <summary>
    /// Управляет показом рекламы в игре.
    /// Используйте в соответствии с гайдлайнами: https://yandex.ru/dev/games/doc/ru/requirements/4/4
    /// </summary>
    public class AdManager
    {
        /// <summary>
        /// Показывает игроку рекламу и выдаёт награду, основываясь на переданном <c>rewardId</c>.
        /// Не рекомендуется задавать rewardId вручную, вместо этого обратитесь к константам класса <see cref="AdRewards"/>
        /// </summary>
        /// <param name="rewardId">Идентификатор награды, выдаваемой за просмотр рекламы</param>
        public static void ShowRewardedAd(string rewardId)
        {
            YG2.RewardedAdvShow(rewardId, () =>
            {
                switch (rewardId)
                {
                    case AdRewards.Coins:
                        SaveManager.Instance.ChangeCoinsAmount(100);
                        break;
                    // ВАЖНО: изменили AdRewards - меняем кейсы
                }
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.Log($"Выдана награда: {rewardId}");
                #endif
            });
        }
    
        /// <summary>
        /// Показывает полноэкранную (Interstitial) рекламу.
        /// </summary>
        public static void ShowFullScreenAd() => YG2.InterstitialAdvShow();
    }
}