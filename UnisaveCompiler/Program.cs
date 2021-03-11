using System;
using DotNetEnv;
using Mono.Unix;
using Mono.Unix.Native;

namespace UnisaveCompiler
{
    // TODO: have the Dockerfile perform compilation
    // TODO: add a user to the dockerfile

    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
                Env.Load();
            else
                Env.Load(args[0]);

            Log.UseColors = Env.GetBool("LOG_USE_COLORS");

            using (var server = new CompilerServer())
            {
                server.Start();

                WaitForTermination();
            }
        }
        
        private static void WaitForTermination()
        {
            if (IsRunningOnMono())
            {
                UnixSignal.WaitAny(GetUnixTerminationSignals());
            }
            else
            {
                Console.WriteLine("Press enter to stop the application.");
                Console.ReadLine();
            }
        }
        
        private static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        private static UnixSignal[] GetUnixTerminationSignals()
        {
            return new[]
            {
                new UnixSignal(Signum.SIGINT),
                new UnixSignal(Signum.SIGTERM),
                new UnixSignal(Signum.SIGQUIT),
                new UnixSignal(Signum.SIGHUP)
            };
        }
    }
}