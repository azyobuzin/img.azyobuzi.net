using System;
using Microsoft.Extensions.DependencyInjection;

namespace ImgAzyobuziNet.TestFramework
{
    public class TestActivator : ITestActivator
    {
        public TestActivator(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        private readonly IServiceProvider serviceProvider;

        public object CreateInstance(Type instanceType, params object[] parameters)
        {
            return ActivatorUtilities.CreateInstance(this.serviceProvider, instanceType, parameters);
        }
    }
}
