namespace BedrockServerWrapper
{
    using System;

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
            PrintTitle();

            if (args.Length < 2)
            {
                if (!Utils.ValidateServerFiles((args.Length == 0) ? string.Empty : args[0]))
                {
                    PrintUsage(ExitCodes.InvalidServerFiles);
                    Environment.Exit(ExitCodes.InvalidServerFiles);
                }

                var serverProcess = new ServerProcess(args.Length == 0 ? string.Empty : args[1]);
                serverProcess.Start();

                while (true)
                {
                    var input = Console.ReadLine();

                    if (input.Equals("stop", StringComparison.CurrentCultureIgnoreCase))
                    {
                        break;
                    }
                    else
                    {
                        serverProcess.SendInputToProcess(input);
                    }
                }

                serverProcess.Stop();
                
                Environment.Exit(ExitCodes.Ok);
            }
            else
            {
                PrintUsage(ExitCodes.InvalidNumberOfArguments);
                Environment.Exit(ExitCodes.InvalidNumberOfArguments);
            }
        }

        private static void PrintTitle()
        {
            Console.WriteLine($"----------------------------------------------");
            Console.WriteLine($"  Minecraft Bedrock Dedicated Server Wrapper  ");
            Console.WriteLine($"  Version: {Utils.ProgramVersion}");
            Console.WriteLine($"----------------------------------------------");
            Console.WriteLine();
        }

        private static void PrintUsage(int exitCode)
        {
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
