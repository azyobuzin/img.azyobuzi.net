using System;

namespace ImgAzyobuziNet.Core.Test
{
    internal class AssertionException : Exception
    {
        public AssertionException(string message) : base(message) { }
    }
}
