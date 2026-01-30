using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Communication;

namespace PVCtrl;

[SupportedOSPlatform("windows6.1")]
public sealed class ObsService : IDisposable
{
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    private const uint WM_CLOSE = 0x0010;

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    private const string DefaultUrl = "ws://localhost:4455";
    private const string DefaultPassword = "";
    private const int RetryIntervalSeconds = 10;

    private static readonly string[] ObsExePaths =
    [
        @"C:\Program Files\obs-studio\bin\64bit\obs64.exe"
    ];

    private readonly OBSWebsocket _obs = new();
    private IDisposable? _tickSubscription;
    private bool _lastProjectorState;
    private int _pollingSuspendCount;

    public bool IsConnected => _obs.IsConnected;
    public bool IsProjectorOpen => FindProjectorWindow(_ => { });

    public event Action<bool>? StatusChanged;
    public event Action<bool>? ProjectorStatusChanged;
    public event Action<bool>? RecordingStatusChanged;

    public ObsService()
    {
        _obs.Connected += OnConnected;
        _obs.Disconnected += OnDisconnected;
        _obs.RecordStateChanged += OnRecordStateChanged;
    }

    private void OnTimerTick()
    {
        if (_pollingSuspendCount > 0) return;

        Connect();

        var isProjectorOpen = IsProjectorOpen;
        if (_lastProjectorState != isProjectorOpen)
        {
            _lastProjectorState = isProjectorOpen;
            ProjectorStatusChanged?.Invoke(isProjectorOpen);
        }

        // プロジェクター非表示、OBS 接続中 → OBS を閉じる
        if (!isProjectorOpen && _obs.IsConnected)
        {
            CloseObs();
        }
    }

    public void Start()
    {
        Connect();
        _tickSubscription = TickService.Subscribe(RetryIntervalSeconds, OnTimerTick);
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
        try
        {
            return _obs.StopRecord();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// OBS を起動し、プロジェクターを表示
    /// </summary>
    public async Task StartObsWithProjectorAsync()
    {
        using var _ = SuspendPolling();

        var process = Process.GetProcesses().FirstOrDefault(p => p.ProcessName == "obs64");
        if (process == null)
            LaunchObs();

        if (!await RetryUntilAsync(() => _obs.IsConnected, maxRetries: 60, delayMs: 500, onRetry: Connect))
            return;

        OpenSourceProjector();
        await RetryUntilAsync(BringProjectorToFront);
        _lastProjectorState = true;
        ProjectorStatusChanged?.Invoke(true);
    }

    /// <summary>
    /// プロジェクターウィンドウを前面に
    /// </summary>
    public static bool BringProjectorToFront()
    {
        return FindProjectorWindow(hWnd => SetForegroundWindow(hWnd));
    }

    /// <summary>
    /// プロジェクターを閉じる
    /// </summary>
    public void CloseProjector()
    {
        FindProjectorWindow(hWnd => PostMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero));
        _lastProjectorState = false;
        ProjectorStatusChanged?.Invoke(false);
    }

    /// <summary>
    /// プロジェクターウィンドウを探してアクションを実行
    /// </summary>
    private static bool FindProjectorWindow(Action<IntPtr> action)
    {
        var obsProcess = Process.GetProcesses().FirstOrDefault(p => p.ProcessName == "obs64");
        if (obsProcess == null) return false;

        var found = false;
        var processId = (uint)obsProcess.Id;
        EnumWindows((hWnd, _) =>
        {
            GetWindowThreadProcessId(hWnd, out var windowProcessId);
            if (windowProcessId != processId) return true;

            var sb = new System.Text.StringBuilder(256);
            GetWindowText(hWnd, sb, sb.Capacity);
            if (sb.ToString().Contains("プロジェクター"))
            {
                action(hWnd);
                found = true;
                return false;
            }

            return true;
        }, IntPtr.Zero);
        return found;
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

    /// <summary>
    /// OBS を起動（トレイに最小化）
    /// </summary>
    private static void LaunchObs()
    {
        foreach (var path in ObsExePaths)
        {
            if (File.Exists(path))
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = "--minimize-to-tray",
                    WorkingDirectory = Path.GetDirectoryName(path)
                };
                Process.Start(startInfo);
                return;
            }
        }
    }

    /// <summary>
    /// OBS を終了
    /// </summary>
    public void CloseObs()
    {
        if (!_obs.IsConnected) return;
        _obs.CallVendorRequest("shutdown-plugin", "shutdown", new JObject
        {
            ["reason"] = "User requested shutdown from ObsCtrl",
            ["support_url"] = "https://github.com/xia-sava/PVCtrl",
            ["force"] = true
        });
        _lastProjectorState = false;
        ProjectorStatusChanged?.Invoke(false);
    }

    private static async Task<bool> RetryUntilAsync(
        Func<bool> condition,
        int maxRetries = 32,
        int delayMs = 100,
        Action? onRetry = null)
    {
        for (var i = 0; i < maxRetries && !condition(); i++)
        {
            await Task.Delay(delayMs);
            onRetry?.Invoke();
        }

        return condition();
    }

    private void OnConnected(object? sender, EventArgs e)
    {
        StatusChanged?.Invoke(true);

        // 接続時に現在の録画状態を取得
        try
        {
            var status = _obs.GetRecordStatus();
            RecordingStatusChanged?.Invoke(status.IsRecording);
        }
        catch
        {
            // 取得失敗は無視
        }
    }

    private void OnRecordStateChanged(object? sender, OBSWebsocketDotNet.Types.Events.RecordStateChangedEventArgs e)
    {
        var isRecording = e.OutputState.State is OBSWebsocketDotNet.Types.OutputState.OBS_WEBSOCKET_OUTPUT_STARTED
            or OBSWebsocketDotNet.Types.OutputState.OBS_WEBSOCKET_OUTPUT_RESUMED;
        RecordingStatusChanged?.Invoke(isRecording);
    }

    private void OnDisconnected(object? sender, ObsDisconnectionInfo info)
    {
        StatusChanged?.Invoke(false);
    }

    public IDisposable SuspendPolling()
    {
        Interlocked.Increment(ref _pollingSuspendCount);
        return new PollingSuspender(this);
    }

    private class PollingSuspender(ObsService service) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Interlocked.Decrement(ref service._pollingSuspendCount);
        }
    }

    public void Dispose()
    {
        _tickSubscription?.Dispose();
        _obs.Connected -= OnConnected;
        _obs.Disconnected -= OnDisconnected;
        _obs.RecordStateChanged -= OnRecordStateChanged;
        Disconnect();
    }
}
