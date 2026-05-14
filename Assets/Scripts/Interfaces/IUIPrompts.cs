namespace Interfaces
{
    public interface IUIPrompts
    {
        /// <summary>
        /// Включает промпты взаимодействия, связанные с объектом
        /// </summary>
        void ShowPrompts();
        
        /// <summary>
        /// Отключает промпты взаимодействия, связанные с объектом
        /// </summary>
        void HidePrompts();
    }
}