using System;
using System.IO;
using System.Threading;
using HyperTomlProcessor;
using ImgAzyobuziV3.Core;
using Nancy.Hosting.Self;

namespace ImgAzyobuziV3
{
    static class Program
    {
        static void Main(string[] args)
        {
            using (var sr = new StreamReader(args[0]))
                MainModule.context = new ImgAzyobuziContext(TomlConvert.DeserializeObject<ImgAzyobuziSettings>(sr));
            var port = Environment.GetEnvironmentVariable("PORT") ?? "61482";
            using (var host = new NancyHost(new UriBuilder("http", "localhost", int.Parse(port)).Uri))
            {
                host.Start();
                Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}
