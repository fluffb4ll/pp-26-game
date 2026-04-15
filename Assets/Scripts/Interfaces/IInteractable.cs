using Player;

namespace Interfaces
{
    /// <summary>
    /// Объявляет общие для всех объектов, с которыми можно взаимодействовать, методы 
    /// </summary>
    public interface IInteractable
    {
        void Interact(PlayerInteraction player);
    }
}
