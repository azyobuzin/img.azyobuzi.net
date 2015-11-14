using System;

namespace ImgAzyobuziNet.Core.Test
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TestMethodAttribute : Attribute
    {
        public TestMethodAttribute(TestType type)
        {
            this.Type = type;
        }

        public TestType Type { get; }
    }
}
