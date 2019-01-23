namespace MCBE_ServerWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public class ServerProcess
    {
        private readonly Process _serverProcess;
        private readonly ConsoleColor _defaultConsoleColor;

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

        public string ServerDirectory { get; }

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

        public void Start()
        {
            _serverProcess.Start();
        }

        public void Stop()
        {
            SendInputToProcess("stop");
        }

        public void AddUserToWhitelist(string name)
        {
            SendInputToProcess($"whitelist add {name}");
        }

        public void RemoveUserFromWhitelist(string name)
        {
            SendInputToProcess($"whitelist remove {name}");
        }

        public List<string> GetOnlineUsers()
        {
            SendInputToProcess("list");
            // TODO: Read list of users from StandardOutput.

            return new List<string>();
        }

        public List<string> GetWhitelist()
        {
            SendInputToProcess("whitelist list");
            // TODO: Read list of whitelist from StandardOutput.

            return new List<string>();
        }
    }
}
