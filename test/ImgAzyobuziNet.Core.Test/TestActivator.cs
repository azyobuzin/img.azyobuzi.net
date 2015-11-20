using System;
using Microsoft.Extensions.DependencyInjection;

namespace ImgAzyobuziNet.Core.Test
{
    public class TestActivator : ITestActivator
    {
        public TestActivator(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        private readonly IServiceProvider serviceProvider;

        public T CreateInstance<T>(params object[] parameters)
        {
            return ActivatorUtilities.CreateInstance<T>(this.serviceProvider, parameters);
        }
    }
}
