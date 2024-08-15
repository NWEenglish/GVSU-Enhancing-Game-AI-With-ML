using System;

namespace Assets.Scripts.Extensions
{
    public static class StringExtensions
    {
        public static void ThrowIfNullOrEmpty(this string value, string paramName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}
