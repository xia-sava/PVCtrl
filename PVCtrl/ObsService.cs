using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Communication;
using static OBSWebsocketDotNet.Types.OutputState;

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

    private static readonly string[] ObsExePaths =
    [
        @"C:\Program Files\obs-studio\bin\64bit\obs64.exe"
    ];

    private readonly OBSWebsocket _obs = new();
    private ITickSubscription? _connectSubscription;
    private ITickSubscription? _projectorSubscription;
    private bool _lastProjectorState;

    private const double AudioSignalTimeoutSeconds = 3.0;
    private const double AudioLevelThreshold = 0.0001; // 約 -80 dB
    private readonly ConcurrentDictionary<string, DateTime> _lastNonZeroAudio = new();
    private bool _lastAudioSignalPresent;
    private CancellationTokenSource? _audioMonitorCts;

    public bool IsConnected => _obs.IsConnected;
    public bool IsProjectorOpen => FindProjectorWindow(_ => { });

    public event Action<bool>? StatusChanged;
    public event Action<bool>? ProjectorStatusChanged;
    public event Action<bool>? RecordingStatusChanged;
    public event Action<bool>? AudioSignalChanged;

    public ObsService()
    {
        _obs.Connected += OnConnected;
        _obs.Disconnected += OnDisconnected;
        _obs.RecordStateChanged += OnRecordStateChanged;
    }

    private void OnConnectTick()
    {
        Connect();
    }

    private void OnProjectorTick()
    {
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
        _connectSubscription = TickService.Subscribe(1, OnConnectTick);
        _projectorSubscription = TickService.Subscribe(3, OnProjectorTick);
        StartAudioMonitor();
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
        using var _ = _projectorSubscription?.Suspend();

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
        var isRecording = e.OutputState.State is OBS_WEBSOCKET_OUTPUT_STARTING
            or OBS_WEBSOCKET_OUTPUT_STARTED
            or OBS_WEBSOCKET_OUTPUT_RESUMED;
        RecordingStatusChanged?.Invoke(isRecording);
    }

    private void OnDisconnected(object? sender, ObsDisconnectionInfo info)
    {
        StatusChanged?.Invoke(false);
        _lastNonZeroAudio.Clear();
        _lastAudioSignalPresent = false;
        AudioSignalChanged?.Invoke(false);
    }

    #region Audio Monitor (別 WebSocket 接続)

    private void StartAudioMonitor()
    {
        StopAudioMonitor();
        _audioMonitorCts = new CancellationTokenSource();
        _ = RunAudioMonitorAsync(_audioMonitorCts.Token);
    }

    private void StopAudioMonitor()
    {
        _audioMonitorCts?.Cancel();
        _audioMonitorCts?.Dispose();
        _audioMonitorCts = null;
        _lastNonZeroAudio.Clear();
        _lastAudioSignalPresent = false;
        AudioSignalChanged?.Invoke(false);
    }

    private async Task RunAudioMonitorAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var ws = new ClientWebSocket();
                await ws.ConnectAsync(new Uri(DefaultUrl), ct);

                // Hello (op=0) を受信
                var hello = await ReceiveJsonAsync(ws, ct);
                if (hello?["op"]?.Value<int>() != 0)
                {
                    return;
                }

                // 認証が必要な場合はスキップ（現状パスワード無し前提）
                if (hello["d"]?["authentication"] != null)
                    return;

                // Identify (op=1) を送信: All + InputVolumeMeters
                const int eventSubscriptionAll = 2047;
                const int inputVolumeMeters = 1 << 16;
                var identify = new JObject
                {
                    ["op"] = 1,
                    ["d"] = new JObject
                    {
                        ["rpcVersion"] = 1,
                        ["eventSubscriptions"] = eventSubscriptionAll | inputVolumeMeters
                    }
                };
                await SendJsonAsync(ws, identify, ct);

                // Identified (op=2) を受信
                var identified = await ReceiveJsonAsync(ws, ct);
                if (identified?["op"]?.Value<int>() != 2)
                    return;

                // イベント受信ループ
                while (!ct.IsCancellationRequested && ws.State == WebSocketState.Open)
                {
                    var msg = await ReceiveJsonAsync(ws, ct);
                    if (msg?["op"]?.Value<int>() != 5) continue;

                    var d = msg["d"] as JObject;
                    if (d?["eventType"]?.ToString() != "InputVolumeMeters") continue;

                    if (d["eventData"] is JObject eventData)
                        ProcessVolumeMetersData(eventData);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch
            {
                // 接続失敗・切断時はリトライ
            }

            if (!ct.IsCancellationRequested)
                await Task.Delay(3000, ct).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        }
    }

    private void ProcessVolumeMetersData(JObject eventData)
    {
        if (eventData["inputs"] is not JArray inputs) return;

        var now = DateTime.UtcNow;
        var inputNames = new List<string>();

        foreach (var inputToken in inputs)
        {
            if (inputToken is not JObject input) continue;

            var inputName = input["inputName"]?.ToString();
            if (inputName == null) continue;
            inputNames.Add(inputName);

            if (input["inputLevelsMul"] is not JArray levels) continue;

            foreach (var channel in levels)
            {
                if (channel is JArray ch && ch.Count >= 3
                    && (ch[2]?.Value<double>() ?? 0) > AudioLevelThreshold)
                {
                    _lastNonZeroAudio[inputName] = now;
                    break;
                }
            }
        }

        var allPresent = inputNames.Count > 0 && inputNames.All(name =>
            _lastNonZeroAudio.TryGetValue(name, out var lastTime) &&
            (now - lastTime).TotalSeconds < AudioSignalTimeoutSeconds);

        if (allPresent != _lastAudioSignalPresent)
        {
            _lastAudioSignalPresent = allPresent;
            AudioSignalChanged?.Invoke(allPresent);
        }
    }

    private static async Task SendJsonAsync(ClientWebSocket ws, JObject obj, CancellationToken ct)
    {
        var bytes = Encoding.UTF8.GetBytes(obj.ToString(Newtonsoft.Json.Formatting.None));
        await ws.SendAsync(bytes, WebSocketMessageType.Text, true, ct);
    }

    private static async Task<JObject?> ReceiveJsonAsync(ClientWebSocket ws, CancellationToken ct)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(65536);
        try
        {
            var totalRead = 0;
            WebSocketReceiveResult result;
            do
            {
                if (totalRead >= buffer.Length)
                {
                    var newBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length * 2);
                    Buffer.BlockCopy(buffer, 0, newBuffer, 0, totalRead);
                    ArrayPool<byte>.Shared.Return(buffer);
                    buffer = newBuffer;
                }

                result = await ws.ReceiveAsync(
                    new ArraySegment<byte>(buffer, totalRead, buffer.Length - totalRead), ct);
                totalRead += result.Count;
            } while (!result.EndOfMessage);

            if (result.MessageType == WebSocketMessageType.Close) return null;

            var json = Encoding.UTF8.GetString(buffer, 0, totalRead);
            return JObject.Parse(json);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    #endregion

    public void Dispose()
    {
        StopAudioMonitor();
        _connectSubscription?.Dispose();
        _projectorSubscription?.Dispose();
        _obs.Connected -= OnConnected;
        _obs.Disconnected -= OnDisconnected;
        _obs.RecordStateChanged -= OnRecordStateChanged;
        Disconnect();
    }
}
