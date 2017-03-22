using System;
using System.Runtime.CompilerServices;

namespace ImgAzyobuziNet.TestFramework
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class TestMethodAttribute : Attribute
    {
        public TestMethodAttribute(TestCategory category, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            this.Category = category;
            this.FilePath = filePath;
            this.LineNumber = lineNumber;
        }

        public TestCategory Category { get; }
        public string FilePath { get; }
        public int LineNumber { get; }
    }
}
