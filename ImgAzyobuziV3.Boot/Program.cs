using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace ImgAzyobuziV3.Boot
{
    static class Program
    {
        static string repo;
        static DirectoryInfo repoDirectory;
        static string configFile;
        static volatile bool isRunning;
        static volatile string appFile;
        static volatile int appPort = -1;
        static volatile string version;
        static volatile string lastBuildLog;
        static readonly bool isRunningOnMono = Type.GetType("Mono.Runtime") != null;

        static void Main(string[] args)
        {
            repo = args[0];
            repoDirectory = Directory.CreateDirectory(args[1]);
            var port = int.Parse(args[2]);
            configFile = args[3];

            BuildAsync();

            var listener = new HttpListener();
            listener.Prefixes.Add(string.Format("http://*:{0}/", port));
            listener.Start();
            while (true)
            {
                ServeAsync(listener.GetContext());
            }
        }

        static Task<Tuple<string, int>> RunCommandAsync(string fileName, string arguments, string workingDir)
        {
            var factory = new TaskCompletionSource<Tuple<string, int>>();

            Task.Run(() =>
            {
                try
                {
                    var sb = new StringBuilder();
                    var p = Process.Start(new ProcessStartInfo()
                    {
                        FileName = fileName,
                        Arguments = arguments,
                        WorkingDirectory = workingDir,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    });
                    DataReceivedEventHandler handler = (sender, e) =>
                    {
                        if (e.Data != null) sb.AppendLine(e.Data);
                    };
                    p.OutputDataReceived += handler;
                    p.ErrorDataReceived += handler;
                    p.Exited += (sender, e) =>
                    {
                        factory.TrySetResult(Tuple.Create(sb.ToString(), p.ExitCode));
                        p.Dispose();
                    };
                }
                catch (Exception ex)
                {
                    factory.TrySetException(ex);
                }
            });

            return factory.Task;
        }

        static async void BuildAsync()
        {
            var sb = new StringBuilder();
            try
            {
                var repoName = Guid.NewGuid().ToString();
                var t = await RunCommandAsync("git", string.Format("clone --recursive {0} {1}", repo, repoName), repoDirectory.FullName).ConfigureAwait(false);
                sb.AppendLine(t.Item1);
                if (t.Item2 != 0)
                    throw new Exception("Failed git clone");

                var dir = Path.Combine(repoDirectory.FullName, repoName);
                using (var wc = new WebClient())
                    await wc.DownloadFileTaskAsync("http://nuget.org/nuget.exe", Path.Combine(dir, "nuget.exe")).ConfigureAwait(false);

                const string nugetArg = "restore ImgAzyobuziV3.sln";
                t = await (isRunningOnMono ? RunCommandAsync("mono", "./nuget.exe " + nugetArg, dir)
                    : RunCommandAsync(Path.Combine(dir, "nuget.exe"), nugetArg, dir)).ConfigureAwait(false);
                sb.AppendLine(t.Item1);
                if (t.Item2 != 0)
                    throw new Exception("Failed nuget restore");

                t = await RunCommandAsync(isRunningOnMono ? "xbuild" : "MSBuild", "ImgAzyobuziV3.sln /p:Configuration=Release", dir).ConfigureAwait(false);
                sb.AppendLine(t.Item1);
                if (t.Item2 != 0)
                    throw new Exception("Failed build");

                appFile = Path.Combine(dir, "ImgAzyobuziV3", "bin", "Release", "ImgAzyobuziV3.exe");
            }
            catch (Exception ex)
            {
                sb.AppendLine(ex.ToString());
            }
            finally
            {
                lastBuildLog = sb.ToString();
            }
        }

        static void Run()
        {

        }

        static void ServeAsync(HttpListenerContext context)
        {
            Task.Run(() =>
            {

            });
        }
    }
}
