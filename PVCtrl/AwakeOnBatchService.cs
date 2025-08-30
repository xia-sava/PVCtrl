using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace PVCtrl;

[SupportedOSPlatform("windows6.1")]
public sealed class AwakeWhileProcessService : IDisposable
{
    [DllImport("kernel32.dll")]
    private static extern uint SetThreadExecutionState(uint esFlags);

    private const uint ES_CONTINUOUS       = 0x80000000;
    private const uint ES_SYSTEM_REQUIRED  = 0x00000001;
    private const uint ES_DISPLAY_REQUIRED = 0x00000002;
    // Away Mode を使いたければ: private const uint ES_AWAYMODE_REQUIRED = 0x00000040;

    private readonly HashSet<string> _targets;
    private readonly bool _keepDisplayOn;
    private readonly object _lock = new();
    private bool _awake;
    private ManagementEventWatcher? _startWatcher;
    private ManagementEventWatcher? _stopWatcher;
    private System.Timers.Timer? _pollTimer;

    public AwakeWhileProcessService(IEnumerable<string> processNames, bool keepDisplayOn = false)
    {
        _targets = new HashSet<string>(
            processNames.Select(n => NormalizeName(n)),
            StringComparer.OrdinalIgnoreCase
        );
        _keepDisplayOn = keepDisplayOn;
    }

    public void Start()
    {
        // 初期状態を反映
        Recalculate();

        // WMI イベント監視（Start/Stop）
        try
        {
            _startWatcher = new ManagementEventWatcher(
                new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
            _startWatcher.EventArrived += (_, __) => Recalculate();
            _startWatcher.Start();

            _stopWatcher = new ManagementEventWatcher(
                new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace"));
            _stopWatcher.EventArrived += (_, __) => Recalculate();
            _stopWatcher.Start();
        }
        catch
        {
            // 権限や WMI サービス停止などで失敗した場合のフォールバック
            _pollTimer = new System.Timers.Timer(2000);
            _pollTimer.Elapsed += (_, __) => Recalculate();
            _pollTimer.AutoReset = true;
            _pollTimer.Start();
        }
    }

    public void UpdateTargets(IEnumerable<string> processNames)
    {
        lock (_lock)
        {
            _targets.Clear();
            foreach (var n in processNames)
                _targets.Add(NormalizeName(n));
        }
        Recalculate();
    }

    public void Dispose()
    {
        lock (_lock)
        {
            try { _startWatcher?.Stop(); } catch { }
            try { _stopWatcher?.Stop(); } catch { }
            _startWatcher?.Dispose();
            _stopWatcher?.Dispose();
            if (_pollTimer is not null)
            {
                _pollTimer.Stop();
                _pollTimer.Dispose();
            }
            SetAwake(false); // 解除して通常へ
        }
    }

    private static string NormalizeName(string name)
        => name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            ? name[..^4]
            : name;

    private void Recalculate()
    {
        bool anyRunning;
        lock (_lock)
        {
            anyRunning = Process.GetProcesses()
                .Any(p => _targets.Contains(p.ProcessName));
        }
        SetAwake(anyRunning);
    }

    private void SetAwake(bool on)
    {
        lock (_lock)
        {
            if (_awake == on) return;

            uint flags = ES_CONTINUOUS | ES_SYSTEM_REQUIRED;
            if (_keepDisplayOn) flags |= ES_DISPLAY_REQUIRED;

            SetThreadExecutionState(on ? flags : ES_CONTINUOUS);
            _awake = on;
            // ここでログや UI 連携（トレイアイコン更新など）を入れてもOK
            Debug.WriteLine(on ? "Awake ON" : "Awake OFF");
        }
    }
}
