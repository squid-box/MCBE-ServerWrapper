namespace MCBE_ServerWrapper
{
    using System;
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using System.Threading;

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
            }

            if (e.Data.Contains("Server started"))
            {
                if (_serverStarting != DateTime.MinValue)
                {
                    Console.WriteLine($"Server started in {(DateTime.Now - _serverStarting).TotalMilliseconds} ms.");
                    return;
                }
            }

            if (e.Data.Contains("Player connected"))
            {
                var playerName = Regex.Match(e.Data, @".* Player connected: (.*), ").Groups[1].Value;

                var thread = new Thread(() =>
                {
                    Thread.Sleep(5000);
                    _serverProcess.Say($"Welcome {playerName}!");

                });
                thread.Start();
            }

            if (e.Data.Contains("Player disconnected"))
            {
                var playerName = Regex.Match(e.Data, @".* Player disconnected: (.*), ").Groups[1].Value;
                _serverProcess.Say($"Goodbye {playerName}!");
            }

            if (e.Data.Contains("Difficulty: ") && !_serverProcess.ServerValues.ContainsKey("Difficulty"))
            {
                _serverProcess.ServerValues.Add("Difficulty", Regex.Match(e.Data, @".*Difficulty: \d (.*)").Groups[1].Value);
            }

            if (e.Data.Contains("Game mode: ") && !_serverProcess.ServerValues.ContainsKey("GameMode"))
            {
                _serverProcess.ServerValues.Add("GameMode", Regex.Match(e.Data, @".*Game mode: \d (.*)").Groups[1].Value);
            }

            if (e.Data.Contains("Level Name: ") && !_serverProcess.ServerValues.ContainsKey("LevelName"))
            {
                _serverProcess.ServerValues.Add("LevelName", Regex.Match(e.Data, @".*Level Name: (.*)").Groups[1].Value);
            }

            if (e.Data.Contains("Version") && !_serverProcess.ServerValues.ContainsKey("ServerVersion"))
            {
                _serverProcess.ServerValues["ServerVersion"] = Regex.Match(e.Data, @".*Version (\d\.\d\.\d\.\d)").Groups[1].Value;
            }

            if (e.Data.Contains("IPv4 supported") && !_serverProcess.ServerValues.ContainsKey("IpV4Port"))
            {
                _serverProcess.ServerValues["IpV4Port"] = Regex.Match(e.Data, @".*port: (\d*)").Groups[1].Value;
            }

            if (e.Data.Contains("IPv6 supported") && !_serverProcess.ServerValues.ContainsKey("IpV6Port"))
            {
                _serverProcess.ServerValues["IpV6Port"] = Regex.Match(e.Data, @".*port: (\d*)").Groups[1].Value;
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
