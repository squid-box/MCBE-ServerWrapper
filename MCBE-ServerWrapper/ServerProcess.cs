namespace MCBE_ServerWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

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
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true
                }
            };

            if (Utils.IsLinux())
            {
                _serverProcess.StartInfo.EnvironmentVariables.Add("LD_LIBRARY_PATH", ".");
            }

            _serverProcess.OutputDataReceived += _serverProcess_OutputDataReceived;
            _serverProcess.ErrorDataReceived += ServerProcessOnErrorDataReceived;
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
        public bool IsRunning
        {
            get
            {
                if (_serverProcess == null)
                {
                    return false;
                }

                return !_serverProcess.HasExited;
            }
        }

        private void ServerProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(e.Data);
            Console.ForegroundColor = _defaultConsoleColor;
        }

        private void _serverProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        private void SendInputToProcess(string input)
        {
            _serverProcess.StandardInput.WriteLine(input);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            _serverProcess.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            SendInputToProcess("stop");
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
