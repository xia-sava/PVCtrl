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
    [ObservableProperty]
    private string filename = "";

    [ObservableProperty]
    private string minUpDown = "30";

    [ObservableProperty]
    private string alarmUpDown = "2";

    [ObservableProperty]
    private bool stopReserveChecked;

    [ObservableProperty]
    private bool closePvReserveChecked;

    [ObservableProperty]
    private string stopTimeLabel = "00:00:00";

    [ObservableProperty]
    private string remainedTimeLabel = "00:00:00";

    [ObservableProperty]
    private string messageText = "";

    [ObservableProperty]
    private string windowTitle = "PvCtrl";

    // プロパティ（Source Generator により自動生成）
    // StopReserveChecked と ClosePvReserveChecked は特別な処理が必要なので partial method で対応

    partial void OnStopReserveCheckedChanged(bool value)
    {
        HandleStopReserveChanged();
    }

    partial void OnClosePvReserveCheckedChanged(bool value)
    {
        PvCtrlUtil.ClosePvReserve = value;
        var mode = value ? "セット" : "解除";
        ShowMessage($"PVクローズ予約を{mode}しました．");
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
    private void Min25() => SetMinutesAndAlarm(25, 1);

    [RelayCommand]
    private void Min30() => SetMinutesAndAlarm(30, 2);

    [RelayCommand]
    private void Min60() => SetMinutesAndAlarm(60, 2);

    [RelayCommand]
    private void Min90() => SetMinutesAndAlarm(90, 2);

    [RelayCommand]
    private void Min120() => SetMinutesAndAlarm(120, 2);

    [RelayCommand]
    private void Min180() => SetMinutesAndAlarm(180, 2);

    private void SetMinutesAndAlarm(int minutes, int alarm)
    {
        MinUpDown = minutes.ToString();
        AlarmUpDown = alarm.ToString();
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

    private void HandleStopReserveChanged()
    {
        if (StopReserveChecked)
        {
            if (!int.TryParse(MinUpDown, out var minutes) ||
                !int.TryParse(AlarmUpDown, out var alarmMinutes)) return;

            var closePv = ClosePvReserveChecked;

            var stopTime = DateTime.Now.AddMinutes(minutes);
            StopTimeLabel = stopTime.ToString("HH:mm:ss");

            PvCtrlUtil.StartRecTimer(
                minutes,
                alarmMinutes,
                _ => {
                    var remaining = stopTime - DateTime.Now;
                    if (remaining.TotalSeconds > 0)
                    {
                        var remainStr = $"{(int)remaining.TotalHours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}";
                        Application.Current.Dispatcher.Invoke(() => {
                            RemainedTimeLabel = remainStr;
                            WindowTitle = $"PvCtrl - 録画停止予約 {remainStr}";
                        });
                    }
                },
                recStop => {
                    Application.Current.Dispatcher.Invoke(() => {
                        if (recStop)
                        {
                            StopTimeLabel = "00:00:00";
                            RemainedTimeLabel = "00:00:00";
                            StopReserveChecked = false;
                            WindowTitle = "PvCtrl";

                            InvokePvMenu(["ファイル", "録画停止"], "予約により録画停止しました．");

                            if (closePv)
                            {
                                // PVを閉じる処理はPvCtrlUtilで処理される
                            }
                        }
                    });
                }
            );
            ShowMessage($"{minutes}分後に録画停止予約を設定しました．");
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

    // ウィンドウクローズ時の処理
    public void OnWindowClosing()
    {
        if (StopReserveChecked)
        {
            PvCtrlUtil.StopRecTimer(false);
        }

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
    }
}
