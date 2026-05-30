using System.Collections.Generic;
using Structs;

namespace YG
{
    /// <summary>
    /// Содержит параметры, которые будут сохраняться в облаке.
    /// Доступ к ним можно получить, используя <c>YG2.saves.[paramName]</c>
    /// </summary>
    /// <remarks>
    /// ВАЖНО: Не изменяйте значения (особенно, число ресурсов) напрямую.
    /// Для этого используйте существующие или создавайте новые методы в менеджерах.
    /// </remarks>
    public partial class SavesYG
    {
        public long lastDailyRewardTime;
        public long lastSaveTime;
        
        public long coins = 5;

        public int currentQuest = 0;

        public int enemyHealthBonus;

        public Dictionary<int, WorkbenchSave> workbenches = new();
    }
}

