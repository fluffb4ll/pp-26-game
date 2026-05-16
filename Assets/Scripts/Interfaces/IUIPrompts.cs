namespace Interfaces
{
    public interface IUIPrompts
    {
        /// <summary>
        /// Включает промпты взаимодействия, связанные с объектом
        /// </summary>
        void ShowInteractionPrompts();
        
        /// <summary>
        /// Отключает промпты взаимодействия, связанные с объектом
        /// </summary>
        void HideInteractionPrompts();
    }
}