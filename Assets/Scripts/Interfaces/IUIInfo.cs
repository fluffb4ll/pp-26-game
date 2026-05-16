using UnityEngine;

namespace Interfaces
{
    public interface IUIInfo
    {
        /// <summary>
        /// Вращает <c>Canvas</c> относительно поворота камеры
        /// </summary>
        /// <param name="rotation">Вращение камеры</param>
        void RotateCanvas(Quaternion rotation);
        
        /// <summary>
        /// Двигает информационную панель в определённом радиусе вокруг объекта относительно позиции игрока
        /// </summary>
        /// <param name="pos">Позиция игрока в мире</param>
        void MoveCanvas(Vector3 pos);
        
        /// <summary>
        /// Возвращает <c>InfoCanvas</c> в его начальную позицию
        /// </summary>
        void ReturnInfoCanvasToDefaultPos();
    }
}