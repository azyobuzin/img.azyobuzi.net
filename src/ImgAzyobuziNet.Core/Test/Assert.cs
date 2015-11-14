using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ImgAzyobuziNet.Core.Test
{
    internal static class Assert
    {
        internal static void Is<T>(this T actual, T expect)
        {
            if (!EqualityComparer<T>.Default.Equals(actual, expect))
                throw new AssertionException($"Actual: {actual}\nExpect: {expect}");
        }

        internal static void True(Expression<Func<bool>> expr)
        {
            if (!expr.Compile().Invoke())
                throw new AssertionException("Not True\n" + expr.Body.ToString());
        }
    }
}
