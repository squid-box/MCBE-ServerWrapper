namespace MCBE_ServerWrapper
{
    using System.Diagnostics;

    public class ServerProcess
    {
        private Process _serverProcess;

        public ServerProcess(string serverDirectory, string serverLaunchCommand)
        {
            ServerDirectory = serverDirectory;
            LaunchCommand = serverLaunchCommand;

            _serverProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = serverLaunchCommand,
                    WorkingDirectory = serverDirectory
                }
            };

            _serverProcess.OutputDataReceived += _serverProcess_OutputDataReceived;
            _serverProcess.ErrorDataReceived +=ServerProcessOnErrorDataReceived;
        }

        private void ServerProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void _serverProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        public string ServerDirectory { get; private set; }

        public string LaunchCommand { get; private set; }

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

        public void Start()
        {
            _serverProcess.Start();
        }
    }
}
