namespace ImgAzyobuziNet.Core.Test
{
    public interface ITestActivator
    {
        T CreateInstance<T>(params object[] parameters);
    }
}
