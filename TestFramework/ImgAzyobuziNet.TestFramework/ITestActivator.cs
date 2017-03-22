using System;

namespace ImgAzyobuziNet.TestFramework
{
    public interface ITestActivator
    {
        object CreateInstance(Type instanceType, params object[] parameters);
    }
}
