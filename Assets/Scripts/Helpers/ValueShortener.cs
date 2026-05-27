using System;
using System.Text;
using Structs;

namespace Helpers
{
    /// <summary>
    /// Хелпер, сокращающий различные значения
    /// </summary>
    public static class ValueShortener
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

        /// <summary>
        /// Сокращает значения таймеров, выделяя часы, минуты и секунды
        /// </summary>
        /// <param name="amount">Количество времени в секундах, требующее обработки</param>
        /// <returns>Строку, отражающую количество часов, минут и секунд, переданных в метод</returns>
        public static string TimeShortener(float amount)
        {
            var seconds = (int)Math.Floor(amount);
            var answer = new StringBuilder();
            
            var hours = seconds / 3600;
            if (hours > 0)
            {
                answer.Append($"{hours} ч. ");
                seconds %= 3600;
            }
            
            var minutes = seconds / 60;
            if (minutes > 0)
            {
                answer.Append($"{minutes} м. ");
                seconds %= 60;
            }
            
            if (seconds >= 0)
                answer.Append($"{seconds} с.");
            
            return answer.ToString().Trim();
        }
    }
}
