namespace AhlSoft.BedrockServerWrapper.PlayerManagement;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using AhlSoft.BedrockServerWrapper.Logging;

/// <inheritdoc cref="IPlayerManager" />
public class PlayerManager : IPlayerManager
{
    private const string PlayerTimeLogFile = @"playertime.json";
    private const string PlayerSeenLogFile = @"playerseen.json";
    private readonly ILog _log;

    public PlayerManager(ILog log)
    {
        _log = log;
        _online = new Dictionary<Player, DateTime>();
        _timeLog = LoadTimeLog();
        _lastSeenLog = LoadSeenLog();
    }

    /// <inheritdoc />
    public event EventHandler<PlayerConnectionEventArgs> PlayerConnected;

    /// <summary>
    /// Collection of currently online <see cref="Player"/> and when they logged in.
    /// </summary>
    private readonly IDictionary<Player, DateTime> _online;

    private readonly IDictionary<string, DateTime> _lastSeenLog;

    /// <summary>
    /// Log of how many minutes <see cref="Player"/>s have played so far.
    /// </summary>
    private readonly IDictionary<string, int> _timeLog;

    /// <inheritdoc />
    public int GetPlayedMinutes(Player player)
    {
        if (!_timeLog.ContainsKey(player.Xuid))
        {
            return -1;
        }

        return _timeLog[player.Xuid];
    }

    /// <inheritdoc />
    public DateTime GetLastSeen(Player player)
    {
        return _lastSeenLog.TryGetValue(player.Xuid, out var timestamp) ? timestamp : DateTime.MinValue;
    }

    /// <inheritdoc />
    public void PlayerLeft(Player player)
    {
        if (!_online.ContainsKey(player))
        {
            _log.Warning($"Player \"{player}\" was not logged in!");
            return;
        }

        if (!_timeLog.ContainsKey(player.Xuid))
        {
            _timeLog.Add(player.Xuid, 0);
        }

        _lastSeenLog[player.Xuid] = DateTime.Now;

        _timeLog[player.Xuid] += Convert.ToInt32(Math.Ceiling((DateTime.UtcNow - _online[player]).TotalMinutes));
        _online.Remove(player);

        SaveTimeLog();
        SaveSeenLog();
    }

    /// <inheritdoc />
    public void PlayerJoined(Player player)
    {
        if (_online.ContainsKey(player))
        {
            _log.Warning($"Player \"{player}\" already logged in!");
            _online.Remove(player);
        }

        _online.Add(player, DateTime.UtcNow);

        PlayerConnected?.Invoke(this, new PlayerConnectionEventArgs(player));
    }

    /// <inheritdoc />
    public int UsersOnline => _online.Count;

    private Dictionary<string, int> LoadTimeLog()
    {
        if (File.Exists(PlayerTimeLogFile))
        {
            _log.Info("Loading player time log from file.");
            return JsonSerializer.Deserialize(File.ReadAllText(PlayerTimeLogFile), PlayerTimeLogContext.Default.DictionaryStringInt32);
        }

        _log.Info("No player time log found, creating new.");
        return new();
    }

    private Dictionary<string, DateTime> LoadSeenLog()
    {
        if (File.Exists(PlayerSeenLogFile))
        {
            _log.Info("Loading player seen log from file.");
            return JsonSerializer.Deserialize(File.ReadAllText(PlayerSeenLogFile), PlayerSeenLogContext.Default.DictionaryStringDateTime);
        }

        _log.Info("No player seen log found, creating new.");
        return new();
    }

    private void SaveTimeLog()
    {
        File.WriteAllText(PlayerTimeLogFile, JsonSerializer.Serialize(_timeLog, PlayerTimeLogContext.Default.DictionaryStringInt32));
    }

    private void SaveSeenLog()
    {
        File.WriteAllText(PlayerSeenLogFile, JsonSerializer.Serialize(_lastSeenLog, PlayerSeenLogContext.Default.DictionaryStringDateTime));
    }
}
