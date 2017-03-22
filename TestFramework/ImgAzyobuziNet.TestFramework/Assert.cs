using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ImgAzyobuziNet.TestFramework
{
    public static class Assert
    {
        public static void Is<T>(this T actual, T expect)
        {
            if (!EqualityComparer<T>.Default.Equals(actual, expect))
                throw new AssertionException($"Actual: {actual}\nExpect: {expect?.ToString() ?? "null"}");
        }

        public static void True(Expression<Func<bool>> expr)
        {
            if (!expr.Compile().Invoke())
                throw new AssertionException("Not True\n" + expr.Body.ToString());
        }

        public static void NotNullOrEmpty(this string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new AssertionException($"The value is null or empty.");
        }
    }
}
