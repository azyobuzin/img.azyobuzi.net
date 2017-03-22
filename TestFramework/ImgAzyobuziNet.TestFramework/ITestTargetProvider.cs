using System.Collections.Generic;
using System.Reflection;

namespace ImgAzyobuziNet.TestFramework
{
    public interface ITestTargetProvider
    {
        IEnumerable<Assembly> GetTargetAssemblies();
        ITestActivator GetActivator();
    }
}
