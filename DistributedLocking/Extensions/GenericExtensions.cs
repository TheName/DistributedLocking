using System;

namespace DistributedLocking.Extensions
{
    internal static class GenericExtensions
    {
        public static void EnsureIsNotNull<T>(this T instance, string paramName)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}