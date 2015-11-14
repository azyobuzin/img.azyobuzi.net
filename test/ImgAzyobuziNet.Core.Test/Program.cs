using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ImgAzyobuziNet.Core.Test
{
    public class Program
    {
        public void Main(string[] args)
        {
            TestType type;
            var methods = new List<string>();

            if (args.Length == 0)
                type = TestType.Static;
            else if (args.Contains("all", StringComparer.OrdinalIgnoreCase))
                type = TestType.Static | TestType.Network;
            else
            {
                type = 0;
                foreach (var x in args)
                {
                    if (x.Contains("."))
                        methods.Add(x);
                    else
                        type |= (TestType)Enum.Parse(typeof(TestType), x, true);
                }
            }

            var testMethods = typeof(ImgAzyobuziNetService).GetTypeInfo().Assembly.DefinedTypes
                .SelectMany(t => t.DeclaredMethods.Where(m =>
                {
                    var attr = m.GetCustomAttribute<TestMethodAttribute>();
                    if (attr == null) return false;
                    if (type.HasFlag(attr.Type)) return true;
                    var s = m.DeclaringType.Name + "." + m.Name;
                    return methods.Any(x => string.Equals(x, s, StringComparison.OrdinalIgnoreCase));
                }))
                .ToArray();

            Console.WriteLine("{0} tests will be run.", testMethods.Length);

            var stopwatch = new Stopwatch();

            foreach (var m in testMethods)
            {
                Console.Write("{0}.{1} ", m.DeclaringType.Name, m.Name);

                if (!m.IsStatic)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("is not a static method");
                    Console.ResetColor();
                    continue;
                }

                if (m.GetParameters().Length > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("has parameters");
                    Console.ResetColor();
                    continue;
                }

                stopwatch.Restart();

                try
                {
                    (m.Invoke(null, null) as Task)?.Wait();

                    stopwatch.Stop();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("OK");
                    Console.ResetColor();
                    Console.WriteLine(" in {0}ms", stopwatch.ElapsedMilliseconds);
                }
                catch (TargetInvocationException ex)
                {
                    stopwatch.Stop();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Failed");
                    Console.ResetColor();
                    Console.WriteLine(" in {0}ms", stopwatch.ElapsedMilliseconds);
                    Console.WriteLine(ex.InnerException.ToString());
                }
            }
        }
    }
}
