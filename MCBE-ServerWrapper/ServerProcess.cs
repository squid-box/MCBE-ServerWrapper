namespace MCBE_ServerWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using System.Threading;

    /// <summary>
    /// 
    /// </summary>
    public class ServerProcess
    {
        private readonly Process _serverProcess;
        private readonly ConsoleColor _defaultConsoleColor;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverDirectory"></param>
        public ServerProcess(string serverDirectory)
        {
            ServerDirectory = serverDirectory;

            _defaultConsoleColor = Console.ForegroundColor;

            _serverProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "bedrock_server",
                    WorkingDirectory = serverDirectory,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true
                }
            };

            if (Utils.IsLinux())
            {
                _serverProcess.StartInfo.EnvironmentVariables.Add("LD_LIBRARY_PATH", ".");
            }

            IpV4Port = -1;
            IpV6Port = -1;
        }

        /// <summary>
        /// 
        /// </summary>
        public string ServerDirectory { get; }

        /// <summary>
        /// 
        /// </summary>
        public string ServerVersion { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int IpV4Port { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int IpV6Port { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsRunning
        {
            get
            {
                if (_serverProcess == null)
                {
                    return false;
                }

                try
                {
                    Process.GetProcessById(_serverProcess.Id);
                    return true;
                }
                catch (ArgumentException)
                {
                    return false;
                }
            }
        }

        private void ServerProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(e.Data);
            Console.ForegroundColor = _defaultConsoleColor;
        }

        private void ServerProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e == null || e.Data == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(ServerVersion) && e.Data.Contains("Version"))
            {
                ServerVersion = Regex.Match(e.Data, @".*Version (\d\.\d\.\d\.\d)").Groups[1].Value;
                Console.WriteLine($"Server version: {ServerVersion}");
                return;
            }

            if (e.Data.Contains("IPv4 supported") && IpV4Port == -1)
            {
                IpV4Port = Convert.ToInt32(Regex.Match(e.Data, @".*port: (\d*)").Groups[1].Value);
                Console.WriteLine($"Server IPv4 port: {IpV4Port}");
                return;
            }

            if (e.Data.Contains("IPv6 supported") && IpV6Port == -1)
            {
                IpV6Port = Convert.ToInt32(Regex.Match(e.Data, @".*port: (\d*)").Groups[1].Value);
                Console.WriteLine($"Server IPv6 port: {IpV6Port}");
                return;
            }

            Console.WriteLine(e.Data);
        }

        private void SendInputToProcess(string input)
        {
            if (IsRunning)
            {
                _serverProcess.StandardInput.WriteLine(input);
                _serverProcess.StandardInput.Flush();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void Say(string message)
        {
            SendInputToProcess($"say {message}");
        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            _serverProcess.Start();

            while (!IsRunning)
            {
                Thread.Sleep(100);
            }

            _serverProcess.OutputDataReceived += ServerProcessOutputDataReceived;
            _serverProcess.ErrorDataReceived += ServerProcessOnErrorDataReceived;

            _serverProcess.BeginOutputReadLine();
            _serverProcess.BeginErrorReadLine();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            SendInputToProcess("stop");
            Console.WriteLine("Shutting down server...");

            _serverProcess.WaitForExit(5000);

            if (!_serverProcess.HasExited)
            {
                Console.Error.WriteLine("Could not exit server process. Killing.");
                _serverProcess.Kill();
            }

            _serverProcess.CancelOutputRead();
            _serverProcess.CancelErrorRead();
            _serverProcess.OutputDataReceived -= ServerProcessOutputDataReceived;
            _serverProcess.ErrorDataReceived -= ServerProcessOnErrorDataReceived;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public void AddUserToWhitelist(string name)
        {
            SendInputToProcess($"whitelist add {name}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public void RemoveUserFromWhitelist(string name)
        {
            SendInputToProcess($"whitelist remove {name}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<string> GetOnlineUsers()
        {
            SendInputToProcess("list");
            // TODO: Read list of users from StandardOutput.

            return new List<string>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<string> GetWhitelist()
        {
            SendInputToProcess("whitelist list");
            // TODO: Read list of whitelist from StandardOutput.

            return new List<string>();
        }
    }
}
