using System;
using System.Media;
using System.Runtime.Versioning;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PVCtrl.Properties;

namespace PVCtrl;

[SupportedOSPlatform("windows6.1")]
public partial class MainViewModel : ObservableObject
{
    private const int MaxRecordingMinutes = 1440; // 最大24時間
    private const int MinRecordingMinutes = 1;
    private const int MaxAlarmMinutes = 1440;
    private const int MinAlarmMinutes = 0;

    [ObservableProperty] private string filename = "";

    private int recordMinutes = 30;

    public int RecordMinutes
    {
        get => recordMinutes;
        set
        {
            var normalized = Math.Max(MinRecordingMinutes, Math.Min(MaxRecordingMinutes, value));
            SetProperty(ref recordMinutes, normalized);
        }
    }

    private int alarmMinutes = 2;

    public int AlarmMinutes
    {
        get => alarmMinutes;
        set
        {
            var normalized = Math.Max(MinAlarmMinutes, Math.Min(MaxAlarmMinutes, value));
            SetProperty(ref alarmMinutes, normalized);
        }
    }

    [ObservableProperty] private bool stopReserveChecked;

    [ObservableProperty] private bool closePvReserveChecked;

    [ObservableProperty] private string stopTimeLabel = "00:00:00";

    [ObservableProperty] private string remainedTimeLabel = "00:00:00";

    [ObservableProperty] private string messageText = "";

    [ObservableProperty] private string windowTitle = "PvCtrl";

    [ObservableProperty] private bool allowSleep;

    private readonly AwakeWhileProcessService _awakeService =
        new(["TMPGEncVMW6Batch", "xyzzy"], pollingIntervalMs: 1000);


    partial void OnStopReserveCheckedChanged(bool value)
    {
        HandleStopReserveChanged(value);
    }

    partial void OnClosePvReserveCheckedChanged(bool value)
    {
        PvCtrlUtil.ClosePvReserve = value;
        var mode = value ? "セット" : "解除";
        ShowMessage($"PVクローズ予約を{mode}しました．");

        if (value)
        {
            PvCtrlUtil.OnPvClosed = () =>
            {
                Application.Current.Dispatcher.Invoke(() => { ClosePvReserveChecked = false; });
            };
        }
        else
        {
            PvCtrlUtil.OnPvClosed = null;
        }
    }

    partial void OnAllowSleepChanged(bool value)
    {
        ShowMessage("バッチ処理中スリープ可否設定を変更しました．");
        _awakeService.SetAllowSleepOnBatch(value);
    }

    public void AdjustRecordMinutes(int delta)
    {
        RecordMinutes += delta;
    }

    public void AdjustAlarmMinutes(int delta)
    {
        AlarmMinutes += delta;
    }

    [RelayCommand]
    private void FilenamePaste()
    {
        if (Clipboard.ContainsText())
        {
            Filename = Clipboard.GetText();
            NormalizeFilename();
        }
    }

    [RelayCommand]
    private void Rec()
    {
        var recFilename = Filename.Trim();
        if (string.IsNullOrEmpty(recFilename))
        {
            recFilename = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            Filename = recFilename;
        }

        PvCtrlUtil.SetSubmitSaveAsDialog(recFilename);
        InvokePvMenu(["ファイル", "録画..."], $"ファイル名「{recFilename}」で録画開始しました．");
    }

    [RelayCommand]
    private void Stop()
    {
        InvokePvMenu(["ファイル", "録画停止"], "録画停止しました．");
        StopReserveChecked = false;
    }

    [RelayCommand]
    private void Min25() => SetRecordAndAlarmMinutes(25, 1);

    [RelayCommand]
    private void Min30() => SetRecordAndAlarmMinutes(30, 2);

    [RelayCommand]
    private void Min60() => SetRecordAndAlarmMinutes(60, 2);

    [RelayCommand]
    private void Min90() => SetRecordAndAlarmMinutes(90, 2);

    [RelayCommand]
    private void Min120() => SetRecordAndAlarmMinutes(120, 2);

    [RelayCommand]
    private void Min180() => SetRecordAndAlarmMinutes(180, 2);

    private void SetRecordAndAlarmMinutes(int recordValue, int alarmValue)
    {
        RecordMinutes = recordValue;
        AlarmMinutes = alarmValue;
    }

    [RelayCommand]
    private void InvokePv()
    {
        PvCtrlUtil.InvokePv();
    }

    [RelayCommand]
    private void LineA()
    {
        InvokePvMenu(["設定", "映像・音声入力端子", "A"], "入力端子 A に切り替えました．");
    }

    [RelayCommand]
    private void LineB()
    {
        InvokePvMenu(["設定", "映像・音声入力端子", "B"], "入力端子 B に切り替えました．");
    }

    [RelayCommand]
    private void SoundOn()
    {
        InvokePvMenu(["設定", "モニタ時に音声を出力"], "音声 on/off を切り替えました．");
    }

    private void HandleStopReserveChanged(bool isChecked)
    {
        if (isChecked)
        {
            var stopTime = DateTime.Now.AddMinutes(RecordMinutes);
            StopTimeLabel = stopTime.ToString("HH:mm:ss");

            PvCtrlUtil.StartRecTimer(
                RecordMinutes,
                AlarmMinutes,
                _ =>
                {
                    var remaining = stopTime - DateTime.Now;
                    if (remaining.TotalSeconds > 0)
                    {
                        var remainStr = $"{(int)remaining.TotalHours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}";
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            RemainedTimeLabel = remainStr;
                            WindowTitle = $"PvCtrl - 録画停止予約 {remainStr}";
                        });
                    }
                },
                recStop =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (recStop)
                        {
                            StopTimeLabel = "00:00:00";
                            RemainedTimeLabel = "00:00:00";
                            StopReserveChecked = false;
                            WindowTitle = "PvCtrl";

                            InvokePvMenu(["ファイル", "録画停止"], "予約により録画停止しました．");
                        }
                        // PVを閉じる処理はPvCtrlUtilで処理される
                    });
                }
            );
            ShowMessage($"{RecordMinutes}分後に録画停止予約を設定しました．");
        }
        else
        {
            PvCtrlUtil.StopRecTimer(false);
            StopTimeLabel = "00:00:00";
            RemainedTimeLabel = "00:00:00";
            WindowTitle = "PvCtrl";
            ShowMessage("録画停止予約を解除しました．");
        }
    }

    private void NormalizeFilename()
    {
        Filename = Filename
            .Trim()
            .Replace('\\', '￥')
            .Replace('/', '／')
            .Replace(':', '：')
            .Replace('?', '？')
            .Replace('*', '＊')
            .Replace('"', '"')
            .Replace("'", "'")
            .Replace('<', '＜')
            .Replace('>', '＞')
            .Replace('|', '｜')
            .Replace('　', ' ')
            .Replace("\n", "")
            .Replace("\r", "");
    }

    private void InvokePvMenu(string[] menuItems, string message = "")
    {
        try
        {
            PvCtrlUtil.ControlMenu(menuItems);
            ShowMessage(message);
        }
        catch (Exception)
        {
            ErrorMessage();
        }
    }

    private void SetMessage(string message)
    {
        MessageText = $"{DateTime.Now:HH:mm:ss} {message}\r\n{MessageText}";
    }

    private void ShowMessage(string message)
    {
        SystemSounds.Hand.Play();
        SetMessage(message);
    }

    private void ErrorMessage(string message = "何らかエラーが発生したっぽい？")
    {
        SystemSounds.Beep.Play();
        SetMessage("Error: " + message);
    }

    // ウィンドウロード時の処理
    public void OnWindowLoaded()
    {
        // ウィンドウの位置・サイズを復元
        try
        {
            var window = Application.Current.MainWindow;
            var bounds = Settings.Default.Bounds;
            if (bounds is { Width: > 0, Height: > 0 })
            {
                window!.Left = bounds.Left;
                window.Top = bounds.Top;
                window.Width = bounds.Width;
                window.Height = bounds.Height;
            }

            if (Settings.Default.WindowState == System.Windows.Forms.FormWindowState.Maximized)
            {
                window!.WindowState = WindowState.Maximized;
            }
        }
        catch
        {
            // 設定読み込みエラーは無視
        }

        _awakeService.Start();
    }

    // ウィンドウクローズ時の処理
    public void OnWindowClosing()
    {
        if (StopReserveChecked)
        {
            PvCtrlUtil.StopRecTimer(false);
        }

        _awakeService.Dispose();

        // ウィンドウの位置・サイズを保存
        try
        {
            var window = Application.Current.MainWindow;
            if (window?.WindowState == WindowState.Normal)
            {
                Settings.Default.Bounds = new System.Drawing.Rectangle(
                    (int)window.Left, (int)window.Top, (int)window.Width, (int)window.Height);
            }

            Settings.Default.WindowState = window?.WindowState == WindowState.Maximized
                ? System.Windows.Forms.FormWindowState.Maximized
                : System.Windows.Forms.FormWindowState.Normal;

            Settings.Default.Save();
        }
        catch
        {
            // 設定保存エラーは無視
        }
    }
}
