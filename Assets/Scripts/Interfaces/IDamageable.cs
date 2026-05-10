using System;

namespace Interfaces
{
    public interface IDamageable
    {
        /// <summary>
        /// Событие получения урона
        /// </summary>
        event Action<float> OnTakeDamage;

        event Action<float> OnHeal;
        
        /// <summary>
        /// Вычитает указанное количество здоровья у персонажа
        /// </summary>
        /// <param name="damageAmount">Вычитаемое количество здоровья</param>
        void TakeDamage(int damageAmount);
        
        /// <summary>
        /// Добавляет указанное количество здоровья персонажу
        /// </summary>
        /// <param name="healAmount">Добавляемое количество здоровья</param>
        void Heal(int healAmount);
        
        /// <summary>
        /// Убивает персонажа
        /// </summary>
        void Die();
    }
}