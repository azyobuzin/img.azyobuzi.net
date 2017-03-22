using System;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace ImgAzyobuziNet.TestFramework.TestAdapter
{
    internal static class Constants
    {
        public const string ExecutorUriString = "executor://img.azyobuzi.net";
        public static readonly Uri ExecutorUri = new Uri(ExecutorUriString);
        public static readonly TestProperty ProviderAssemblyNameProperty = TestProperty.Register("ImgAzyobuziNet.ProviderAssemblyName", "ProviderAssemblyName", typeof(string), TestPropertyAttributes.Hidden, typeof(TestCase));
        public static readonly TestProperty ProviderTypeFullNameProperty = TestProperty.Register("ImgAzyobuziNet.ProviderTypeFullName", "ProviderTypeFullName", typeof(string), TestPropertyAttributes.Hidden, typeof(TestCase));
        public static readonly TestProperty TestMethodAssemblyNameProperty = TestProperty.Register("ImgAzyobuziNet.TestMethodAssemblyName", "TestMethodAssemblyName", typeof(string), TestPropertyAttributes.Hidden, typeof(TestCase));
        public static readonly TestProperty TestMethodDeclaringTypeProperty = TestProperty.Register("ImgAzyobuziNet.TestMethodDeclaringType", "TestMethodDeclaringType", typeof(string), TestPropertyAttributes.Hidden, typeof(TestCase));
        public static readonly TestProperty TestMethodNameProperty = TestProperty.Register("ImgAzyobuziNet.TestMethodName", "TestMethodName", typeof(string), TestPropertyAttributes.Hidden, typeof(TestCase));
    }
}
