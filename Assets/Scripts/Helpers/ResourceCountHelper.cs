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
        /// <param name="postfix">Постфикс, добавляемый к <see cref="ShortenedValue.formatTemplate"/></param>
        /// <returns>Сокращённое значение формата <see cref="ShortenedValue"/></returns>
        public static ShortenedValue CountShortener(long amount, string prefix = null, string postfix = null)
        {
            return amount switch
            {
                > 1000000000 => new ShortenedValue((float) (amount / 1000000000.0), prefix + "{0:1}B" + postfix),
                > 1000000 => new ShortenedValue((float) (amount / 1000000.0), prefix + "{0:1}M" + postfix),
                > 10000 => new ShortenedValue((float) (amount / 1000.0), prefix + "{0:1}K" + postfix),
                _ => new ShortenedValue(amount, prefix + "{0:0}" + postfix)
            };
        } 
    }
}
