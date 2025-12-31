using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Communication;

namespace PVCtrl;

[SupportedOSPlatform("windows6.1")]
public sealed class ObsService : IDisposable
{
    private const string DefaultUrl = "ws://localhost:4455";
    private const string DefaultPassword = "";
    private const int RetryIntervalSeconds = 10;

    private static readonly string[] ObsExePaths =
    [
        @"C:\Program Files\obs-studio\bin\64bit\obs64.exe"
    ];

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

    /// <summary>
    /// 録画ファイル名を設定して録画開始
    /// </summary>
    public void StartRecord(string filename)
    {
        if (!_obs.IsConnected) return;
        _obs.SetProfileParameter("Output", "FilenameFormatting", filename);
        _obs.StartRecord();
    }

    /// <summary>
    /// 録画停止
    /// </summary>
    public string? StopRecord()
    {
        if (!_obs.IsConnected) return null;
        return _obs.StopRecord();
    }

    /// <summary>
    /// OBS の起動/ウィンドウ表示トグル
    /// </summary>
    public async void InvokeObs()
    {
        var process = Process.GetProcesses().FirstOrDefault(p => p.ProcessName == "obs64");
        if (process != null)
        {
            ToggleWindowState(process);
            Connect();
        }
        else
        {
            LaunchObs();
            // OBS の WebSocket サーバー起動を待ってから接続
            await Task.Delay(1500);
            Connect();
        }
    }

    /// <summary>
    /// OBS を終了
    /// </summary>
    public void CloseObs()
    {
        var process = Process.GetProcesses().FirstOrDefault(p => p.ProcessName == "obs64");
        process?.CloseMainWindow();
    }

    /// <summary>
    /// 現在のシーンの先頭ソースをプロジェクター表示
    /// </summary>
    public bool OpenSourceProjector()
    {
        if (!_obs.IsConnected) return false;
        try
        {
            var currentScene = _obs.GetCurrentProgramScene();
            var sceneItems = _obs.GetSceneItemList(currentScene);
            if (sceneItems.Count == 0) return false;

            var sourceName = sceneItems[0].SourceName;
            // Qt geometry: (2, 28, 961, 568) - DPI 200% 環境用
            const string geometry =
                "AdnQywADAAAAAAACAAAAHAAAA8EAAAI4AAAAAgAAABwAAAPBAAACOAAAAAAAAAAAB4AAAAACAAAAHAAAA8EAAAI4";

            _obs.SendRequest("OpenSourceProjector", new JObject
            {
                ["sourceName"] = sourceName,
                ["projectorGeometry"] = geometry
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void ToggleWindowState(Process process)
    {
        try
        {
            var element = AutomationElement.FromHandle(process.MainWindowHandle);
            var windowPattern = (WindowPattern)element.GetCurrentPattern(WindowPattern.Pattern);
            var currentState = windowPattern.Current.WindowVisualState;
            windowPattern.SetWindowVisualState(
                currentState == WindowVisualState.Minimized
                    ? WindowVisualState.Normal
                    : WindowVisualState.Minimized);
        }
        catch
        {
            // ウィンドウ操作失敗は無視
        }
    }

    private static void LaunchObs()
    {
        foreach (var path in ObsExePaths)
        {
            if (File.Exists(path))
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = path,
                    WorkingDirectory = Path.GetDirectoryName(path)
                };
                Process.Start(startInfo);
                return;
            }
        }
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
