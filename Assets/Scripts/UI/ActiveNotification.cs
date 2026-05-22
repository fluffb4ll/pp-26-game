using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Уведомления, выводимые игроком
    /// </summary>
    public class ActiveNotification : MonoBehaviour
    {
        [SerializeField] private GameObject notification;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI message;
        [SerializeField] private float lifeTimeLeft;

        /// <summary>
        /// Изменяет текст уведомления
        /// </summary>
        /// <param name="newValue">Новый текст уведомления</param>
        public void SetMessage(string newValue) => message.SetText(newValue);
        
        /// <summary>
        /// Возвращает оставшееся время жизни уведомления
        /// </summary>
        public float GetLifeTimeLeft() => lifeTimeLeft;
        
        /// <summary>
        /// Устанавливает время жизни уведомления
        /// </summary>
        /// <param name="lifeTime">Новое значение времени жизни</param>
        public void SetLifeTime(float lifeTime) => lifeTimeLeft = lifeTime;
        
        /// <summary>
        /// Уменьшает время жизни на заданное значение
        /// </summary>
        /// <param name="amount">Вычитатель времени жизни</param>
        /// <returns>Оставшееся время жизни</returns>
        public float DecreaseLifetime(float amount) => lifeTimeLeft -= amount;
        
        /// <summary>
        /// Изменяет спрайт иконки уведомления
        /// </summary>
        /// <param name="newIcon">Новый спрайт иконки</param>
        public void SetIcon(Sprite newIcon) => icon.sprite = newIcon;
        
        /// <summary>
        /// Возвращает объект-уведомление
        /// </summary>
        public GameObject GetNotification() => notification;
    }
}
