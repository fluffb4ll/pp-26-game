using System;
using Player;
using Structs;
using UnityEngine;

namespace Interfaces
{
    /// <summary>
    /// Объявляет общие для всех объектов, с которыми можно взаимодействовать, методы 
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Метод, вызываемый при взаимодействии игрока с объектом, реализующим интерфейс <see cref="IInteractable"/>
        /// </summary>
        /// <param name="player">Компонент <see cref="PlayerInteraction"/> игрока, вызвавшего взаимодействие</param>
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
