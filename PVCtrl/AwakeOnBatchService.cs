using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace PVCtrl;

[SupportedOSPlatform("windows6.1")]
public sealed class AwakeWhileProcessService(
    IEnumerable<string> processNames,
    int pollingIntervalMs = 2000) : IDisposable
{
    [DllImport("kernel32.dll")]
    private static extern uint SetThreadExecutionState(uint esFlags);

    private const uint ES_CONTINUOUS = 0x80000000;
    private const uint ES_SYSTEM_REQUIRED = 0x00000001;
    private const uint ES_DISPLAY_REQUIRED = 0x00000002;

    private System.Timers.Timer _pollTimer = new(pollingIntervalMs);

    private readonly HashSet<string> _targets = new(
        processNames.Select(name => name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            ? name[..^4]
            : name),
        StringComparer.OrdinalIgnoreCase
    );

    private bool _allowSleepOnBatch; // スリープ可ボタン

    public void Start()
    {
        _pollTimer.Elapsed += (_, _) =>
        {
            UpdateAwakeState();
            _pollTimer.Start();
        };
        _pollTimer.AutoReset = false;
        _pollTimer.Start();
    }

    public void Dispose()
    {
        _pollTimer.Stop();
        _pollTimer.Dispose();
        SetThreadExecutionState(ES_CONTINUOUS);
    }

    public void SetAllowSleepOnBatch(bool allowSleepOnBatch)
    {
        _allowSleepOnBatch = allowSleepOnBatch;
    }

    private bool IsProcessRunning()
    {
        return Process.GetProcesses().Any(p => _targets.Contains(p.ProcessName));
    }

    private void UpdateAwakeState()
    {
        // 現在必要な状態 = プロセス動作中 AND NOT スリープ可
        SetThreadExecutionState(
            IsProcessRunning() && !_allowSleepOnBatch
                ? ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED
                : ES_CONTINUOUS);
    }
}
