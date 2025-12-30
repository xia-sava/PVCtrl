using System;
using System.Runtime.Versioning;
using System.Windows.Threading;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Communication;

namespace PVCtrl;

[SupportedOSPlatform("windows6.1")]
public sealed class ObsService : IDisposable
{
    private const string DefaultUrl = "ws://localhost:4455";
    private const string DefaultPassword = "";
    private const int RetryIntervalSeconds = 10;

    private readonly OBSWebsocket _obs = new();
    private readonly DispatcherTimer _retryTimer = new() { Interval = TimeSpan.FromSeconds(RetryIntervalSeconds) };

    public bool IsConnected => _obs.IsConnected;

    public event Action<bool>? StatusChanged;

    public ObsService()
    {
        _obs.Connected += OnConnected;
        _obs.Disconnected += OnDisconnected;
        _retryTimer.Tick += (_, _) => Connect();
    }

    public void Start()
    {
        Connect();
        _retryTimer.Start();
    }

    public void Connect()
    {
        if (_obs.IsConnected) return;
        try
        {
            _obs.ConnectAsync(DefaultUrl, DefaultPassword);
        }
        catch
        {
            // 接続失敗は無視（次回リトライで再試行）
        }
    }

    public void Disconnect()
    {
        if (!_obs.IsConnected) return;
        _obs.Disconnect();
    }

    private void OnConnected(object? sender, EventArgs e)
    {
        StatusChanged?.Invoke(true);
    }

    private void OnDisconnected(object? sender, ObsDisconnectionInfo info)
    {
        StatusChanged?.Invoke(false);
    }

    public void Dispose()
    {
        _retryTimer.Stop();
        _obs.Connected -= OnConnected;
        _obs.Disconnected -= OnDisconnected;
        Disconnect();
    }
}
