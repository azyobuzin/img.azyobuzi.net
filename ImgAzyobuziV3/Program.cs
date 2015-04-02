using System;
using System.IO;
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
            var listenUri = new UriBuilder("http", "localhost", int.Parse(port)).Uri;
            using (var host = new NancyHost(listenUri))
            {
                host.Start();
                Console.WriteLine(listenUri.AbsoluteUri);
                Console.ReadLine();
            }
        }
    }
}
