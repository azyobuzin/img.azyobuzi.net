using System;

namespace ImgAzyobuziNet.TestFramework
{
    public class AssertionException : Exception
    {
        public AssertionException(string message) : base(message) { }
    }
}
