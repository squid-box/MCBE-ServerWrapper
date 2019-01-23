namespace MCBE_ServerWrapper
{
    using System;
    using System.Threading;

    /// <summary>
    /// 
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                if (!Utils.ValidateServerFiles((args.Length == 0) ? string.Empty : args[0]))
                {
                    PrintUsage(ExitCodes.InvalidServerFiles);
                    Environment.Exit(ExitCodes.InvalidServerFiles);
                }

                var serverProcess = new ServerProcess(args.Length == 0 ? string.Empty : args[1]);
                serverProcess.Start();

                while (serverProcess.IsRunning)
                {
                    Thread.Sleep(100);
                }

                Environment.Exit(ExitCodes.Ok);
            }
            else
            {
                PrintUsage(ExitCodes.InvalidNumberOfArguments);
                Environment.Exit(ExitCodes.InvalidNumberOfArguments);
            }
        }

        private static void PrintUsage(int exitCode)
        {
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine("  Minecraft Bedrock Dedicated Server Wrapper  ");
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine();

            if (exitCode == ExitCodes.InvalidNumberOfArguments)
            {
                Console.WriteLine("Usage: start with either:");
                Console.WriteLine(" * Without any arguments, if running in the server directory.");
                Console.WriteLine(" * Without one argument, pointing out the server directory.");
            }
            else if (exitCode == ExitCodes.InvalidServerFiles)
            {
                Console.WriteLine("Server files broken / missing, please check and retry.");
            }
        }
    }
}
