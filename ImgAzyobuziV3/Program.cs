using System;
using System.Threading;
using Nancy.Hosting.Self;

namespace ImgAzyobuziV3
{
    static class Program
    {
        public static string ConfigFile { get; set; }

        static void Main(string[] args)
        {
            ConfigFile = args[0];
            var port = Environment.GetEnvironmentVariable("PORT") ?? "61482";
            using (var host = new NancyHost(new UriBuilder("http", "localhost", int.Parse(port)).Uri))
            {
                host.Start();
                Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}
