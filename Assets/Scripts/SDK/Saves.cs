namespace YG
{
    /// <summary>
    /// Содержит параметры, которые будут сохранятся в облаке.
    /// Доступ к ним можно получить, используя <c>YG.saves.[paramName]</c>
    /// </summary>
    public partial class SavesYG
    {
        // TODO: разобраться, нужно ли инициализировать значения
        public long lastDailyRewardTime;
        public int coins = 5;
    }
}

