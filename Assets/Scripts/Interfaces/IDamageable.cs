namespace Interfaces
{
    public interface IDamageable
    {
        /// <summary>
        /// Вычитает указанное количество здоровья у персонажа
        /// </summary>
        /// <param name="damageAmount">Вычитаемое количество здоровья</param>
        void TakeDamage(int damageAmount);
        
        /// <summary>
        /// Убивает персонажа
        /// </summary>
        void Die();
    }
}