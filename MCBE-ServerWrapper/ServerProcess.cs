namespace BedrockServerWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    using Backups;

    /// <summary>
    /// 
    /// </summary>
    public class ServerProcess : IDisposable
    {
        private readonly Process _serverProcess;
        private readonly InputOutputManager _inputOutputManager;
        private readonly BackupManager _backupManager;
        private readonly Log _log;
        private readonly Settings _settings;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverDirectory"></param>
        /// <param name="log"></param>
        /// <param name="settings"></param>
        public ServerProcess(string serverDirectory, Log log, Settings settings)
        {
            ServerDirectory = serverDirectory;
            ServerValues = new Dictionary<string, string>();

            _settings = settings;
            _log = log;

            _serverProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = serverDirectory,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                }
            };

            if (Utils.IsLinux())
            {
                _serverProcess.StartInfo.FileName = "bedrock_server";
                _serverProcess.StartInfo.EnvironmentVariables.Add("LD_LIBRARY_PATH", ".");
            }
            else
            {
                _serverProcess.StartInfo.FileName = "bedrock_server.exe";
            }

            _inputOutputManager = new InputOutputManager(log, settings, this);
            _backupManager = new BackupManager(log, _settings);
            _backupManager.BackupCompleted += _inputOutputManager.BackupCompleted;

            _inputOutputManager.BackupReady += _backupManager.ManualBackup;
            _inputOutputManager.PlayerJoined += _backupManager.PlayerJoined;
        }

        /// <summary>
        /// Contains any properties and values related to the server.
        /// </summary>
        public Dictionary<string, string> ServerValues { get; }

        /// <summary>
        /// 
        /// </summary>
        public string ServerDirectory { get; }

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        public void SendInputToProcess(string input)
        {
            if (IsRunning)
            {
                _serverProcess.StandardInput.WriteLine(input);
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
        /// Initiates a backup.
        /// </summary>
        public void Backup()
        {
            _backupManager.HasBackupBeenInitiated = true;
            SendInputToProcess("save hold");
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

            _serverProcess.OutputDataReceived += _inputOutputManager.ReceivedStandardOutput;
            _serverProcess.ErrorDataReceived += _inputOutputManager.ReceivedErrorOutput;

            _serverProcess.BeginOutputReadLine();
            _serverProcess.BeginErrorReadLine();
            _serverProcess.StandardInput.AutoFlush = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            SendInputToProcess("stop");
            _log.Info("Shutting down server...");

            _serverProcess.WaitForExit(5000);

            if (!_serverProcess.HasExited)
            {
                _log.Error("Could not exit server process. Killing.");
                _serverProcess.Kill();
            }

            _serverProcess.CancelOutputRead();
            _serverProcess.CancelErrorRead();
            _serverProcess.OutputDataReceived -= _inputOutputManager.ReceivedStandardOutput;
            _serverProcess.ErrorDataReceived -= _inputOutputManager.ReceivedErrorOutput;
        }

        public void PrintServerValues()
        {
            _log.Info("Server values:");

            foreach (var serverValue in ServerValues)
            {
                _log.Info($" * {serverValue.Key} : {serverValue.Value}");
            }
        }

        /// <summary>
        /// Dispose all resources.
        /// </summary>
        /// <param name="disposing">Whether or not we're disposing.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
                _settings.Save();

				_inputOutputManager.BackupReady -= _backupManager.ManualBackup;
				_inputOutputManager.PlayerJoined -= _backupManager.PlayerJoined;

                _inputOutputManager?.Dispose();
                _serverProcess?.Dispose();
			}
        }

        /// <inheritdoc />
        public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}