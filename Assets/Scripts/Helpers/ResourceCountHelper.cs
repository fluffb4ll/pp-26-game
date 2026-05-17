using Structs;

namespace Helpers
{
    /// <summary>
    /// Хелпер, обрабатывающий счётчики ресурсов
    /// </summary>
    public static class ResourceCountHelper
    {
        /// <summary>
        /// Сокращает значение счётчика ресурсов, добавляя постфиксы, где
        /// "K" - тысячи, "M" - миллионы, "B" - миллиарды
        /// </summary>
        /// <param name="amount">Значение, которое нужно сократить</param>
        /// <param name="prefix">Префикс, добавляемый к <see cref="ShortenedValue.formatTemplate"/></param>
        /// <returns>Сокращённое значение формата <see cref="ShortenedValue"/></returns>
        public static ShortenedValue CountShortener(long amount, string prefix = null)
        {
            return amount switch
            {
                > 1000000000 => new ShortenedValue((float) (amount / 1000000000.0), prefix + "{0:1}B"),
                > 1000000 => new ShortenedValue((float) (amount / 1000000.0), prefix + "{0:1}M"),
                > 10000 => new ShortenedValue((float) (amount / 1000.0), prefix + "{0:1}K"),
                _ => new ShortenedValue(amount, prefix + "{0:0}")
            };
        } 
    }
}
