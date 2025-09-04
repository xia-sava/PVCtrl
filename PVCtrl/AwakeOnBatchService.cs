using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Windows.Automation;
using System.Windows.Threading;

namespace PVCtrl;

[SupportedOSPlatform("windows6.1")]
public sealed class AwakeOnBatchService : IDisposable
{
    private const int pollInterval = 10;

    [DllImport("kernel32.dll")]
    private static extern uint SetThreadExecutionState(uint esFlags);

    private const uint ES_CONTINUOUS = 0x80000000;
    private const uint ES_SYSTEM_REQUIRED = 0x00000001;
    private const uint ES_DISPLAY_REQUIRED = 0x00000002;

    private readonly DispatcherTimer _pollTimer = new() { Interval = TimeSpan.FromSeconds(pollInterval) };

    public event Action<bool> StatusChanged = _ => { }; // スリープ抑止状態変化通知
    private bool _lastAwakeState;

    public void Start()
    {
        _pollTimer.Tick += (_, _) => UpdateAwakeState();
        _pollTimer.Start();
    }

    public void Dispose()
    {
        _pollTimer.Stop();
        SetThreadExecutionState(ES_CONTINUOUS);
    }

    private bool IsEncoding()
    {
        // バッチプロセスの存在確認
        var batchProcess = Process.GetProcessesByName("TMPGEncVMW6Batch").FirstOrDefault();
        if (batchProcess is null)
            return false;
        try
        {
            // TBatch_InnerFrame_EncodeJobFrameの存在確認
            return AutomationElement
                .FromHandle(batchProcess.MainWindowHandle)
                .FindFirst(TreeScope.Descendants,
                    new PropertyCondition(
                        AutomationElement.ClassNameProperty,
                        "TBatch_InnerFrame_EncodeJobFrame"
                    )
                ) is not null;
        }
        catch
        {
            // UI Automation失敗
        }

        // エンコード中であると認識できなかった時は false
        return false;
    }

    private void UpdateAwakeState()
    {
        // エンコード中であればスリープ抑止
        var shouldAwake = IsEncoding();

        // スリープ抑止状態を通知
        if (_lastAwakeState != shouldAwake)
        {
            _lastAwakeState = shouldAwake;
            StatusChanged(shouldAwake);
        }

        SetThreadExecutionState(
            shouldAwake
                ? ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED
                : ES_CONTINUOUS);
    }
}
