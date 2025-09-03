using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Windows.Automation;

namespace PVCtrl;

[SupportedOSPlatform("windows6.1")]
public sealed class AwakeWhileProcessService : IDisposable
{
    private const int pollInterval = 10_000;

    [DllImport("kernel32.dll")]
    private static extern uint SetThreadExecutionState(uint esFlags);

    private const uint ES_CONTINUOUS = 0x80000000;
    private const uint ES_SYSTEM_REQUIRED = 0x00000001;
    private const uint ES_DISPLAY_REQUIRED = 0x00000002;

    private System.Timers.Timer _pollTimer = new(pollInterval);

    private bool _allowSleepOnBatch; // スリープ可ボタン
    private bool _lastEncodingState; // 前回のエンコード状態

    public event Action<string> StatusChanged = _ => { }; // 状態変化通知（空のハンドラで初期化）

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
        var isEncoding = IsEncoding();

        // 状態変化を検出してメッセージ出力
        if (_lastEncodingState != isEncoding)
        {
            _lastEncodingState = isEncoding;
            StatusChanged(isEncoding
                ? "TMPGEncのエンコードキューを検出 - スリープ防止ON"
                : "TMPGEncのエンコードキューが空になりました - スリープ防止OFF");
        }

        // 現在必要な状態 = エンコード中 AND NOT スリープ可
        SetThreadExecutionState(
            isEncoding && !_allowSleepOnBatch
                ? ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED
                : ES_CONTINUOUS);
    }
}
