namespace Helpers
{
    public class HashFunctions
    {
        /// <summary>
        /// Использует алгоритм FNV-1a для нахождения детерминированного хэша
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int GetDeterministicHashCode(string str)
        {
            unchecked
            {
                var hash = (int)2166136261;
                foreach (var c in str)
                    hash = (hash ^ c) * 16777619;
                return hash;
            }
        }
    }
}