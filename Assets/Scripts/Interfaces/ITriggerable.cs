using Player;

namespace Interfaces
{
    public interface ITriggerable
    {
        /// <summary>
        /// Исполняет метод при входе игрока в триггер
        /// </summary>
        /// <param name="playerController">Компонент <see cref="PlayerController"/></param>
        void Execute(PlayerController playerController);
        
        /// <summary>
        /// Метод, исполняемый при выходе игрока из триггера
        /// </summary>
        /// <param name="playerController">Компонент <see cref="PlayerController"/></param>
        void Exit(PlayerController playerController);
    }
}