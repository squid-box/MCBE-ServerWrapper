namespace MCBE_ServerWrapper
{
    using System;
    using System.Diagnostics;
    using System.Text.RegularExpressions;

    /// <summary>
    /// 
    /// </summary>
    public class InputOutputManager
    {
        private readonly ServerProcess _serverProcess;
        private readonly ConsoleColor _defaultConsoleColor;

        private DateTime _serverStarting;

        public InputOutputManager(ServerProcess serverProcess)
        {
            _serverProcess = serverProcess;
            _serverStarting = DateTime.MinValue;
            _defaultConsoleColor = Console.ForegroundColor;
        }

        public void ParseInput(string input)
        {

        }

        public void ReceivedStandardOutput(object sender, DataReceivedEventArgs e)
        {
            if (e?.Data == null)
            {
                return;
            }

            if (e.Data.Contains("Starting Server"))
            {
                _serverStarting = DateTime.Now;
                Console.WriteLine("Server starting...");
                return;
            }

            if (e.Data.Contains("Server started"))
            {
                if (_serverStarting == DateTime.MinValue)
                {
                    Console.WriteLine("Server started.");
                }
                else
                {
                    Console.WriteLine($"Server started in {(DateTime.Now - _serverStarting).TotalSeconds} seconds.");
                }

                return;
            }

            if (e.Data.Contains("Player connected"))
            {
                var playerName = Regex.Match(e.Data, $".* Player connected: (.*), ").Groups[1].Value;
                Console.WriteLine($"Player \"{playerName}\" connected.");
                _serverProcess.Say($"Welcome {playerName}!");

                return;
            }

            if (e.Data.Contains("Player disconnected"))
            {
                var playerName = Regex.Match(e.Data, $".* Player disconnected: (.*), ").Groups[1].Value;
                Console.WriteLine($"Player \"{playerName}\" disconnected.");
                _serverProcess.Say($"Goodbye {playerName}!");

                return;
            }

            if (e.Data.Contains("Version") && !_serverProcess.ServerValues.ContainsKey("ServerVersion"))
            {
                _serverProcess.ServerValues["ServerVersion"] = Regex.Match(e.Data, @".*Version (\d\.\d\.\d\.\d)").Groups[1].Value;
                Console.WriteLine($"Server version: {_serverProcess.ServerValues["ServerVersion"]}");

                return;
            }

            if (e.Data.Contains("IPv4 supported") && !_serverProcess.ServerValues.ContainsKey("IpV4Port"))
            {
                _serverProcess.ServerValues["IpV4Port"] = Regex.Match(e.Data, @".*port: (\d*)").Groups[1].Value;
                Console.WriteLine($"Server IPv4 port: {_serverProcess.ServerValues["IpV4Port"]}");

                return;
            }

            if (e.Data.Contains("IPv6 supported") && !_serverProcess.ServerValues.ContainsKey("IpV6Port"))
            {
                _serverProcess.ServerValues["IpV6Port"] = Regex.Match(e.Data, @".*port: (\d*)").Groups[1].Value;
                Console.WriteLine($"Server IPv6 port: {_serverProcess.ServerValues["IpV6Port"]}");

                return;
            }

            Console.WriteLine(e.Data);
        }

        public void ReceivedErrorOutput(object sender, DataReceivedEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(e.Data);
            Console.ForegroundColor = _defaultConsoleColor;
        }
    }
}
