using Player;
using UnityEngine;

namespace Interfaces
{
    /// <summary>
    /// Объявляет общие для всех объектов, с которыми можно взаимодействовать, методы 
    /// </summary>
    public interface IInteractable
    {
        void Interact(PlayerInteraction player);
        
        /// <summary>
        /// Возвращает компонент, управляющий интерфейсом объекта 
        /// </summary>
        /// <returns>Объект, реализующий интерфейс <see cref="IUIPrompts"/></returns>
        IUIPrompts GetUIComponent();
        
        /// <summary>
        /// Возвращает позицию объекта в мире
        /// </summary>
        Vector3 GetPosition();
    }
}
