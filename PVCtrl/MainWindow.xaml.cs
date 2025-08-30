using System;
using System.Media;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PVCtrl.Properties;

namespace PVCtrl;

[SupportedOSPlatform("windows6.1")]
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        SetupNumericTextBoxes();
    }

    private void SetupNumericTextBoxes()
    {
        // NumericUpDown代替のTextBoxに数値のみ入力を許可
        MinUpDown.PreviewTextInput += NumericTextBox_PreviewTextInput;
        AlarmUpDown.PreviewTextInput += NumericTextBox_PreviewTextInput;

        // 初期値設定
        MinUpDown.Text = "30";
        AlarmUpDown.Text = "2";
    }

    private static void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // 数字のみ許可
        e.Handled = !IsNumeric(e.Text);
    }

    private static bool IsNumeric(string text) => int.TryParse(text, out _);

    private void NumericUpDown_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not TextBox textBox) return;
        if (!int.TryParse(textBox.Text, out var value)) return;

        var newValue = e.Delta > 0 ? value + 1 : value - 1;

        // 範囲チェック
        if (textBox == MinUpDown)
        {
            newValue = Math.Max(1, Math.Min(1440, newValue));
        }
        else if (textBox == AlarmUpDown)
        {
            newValue = Math.Max(0, Math.Min(1440, newValue));
        }

        textBox.Text = newValue.ToString();
        e.Handled = true;
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
        MessageTextBox.Text = $"{DateTime.Now:HH:mm:ss} {message}\r\n{MessageTextBox.Text}";
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

    // イベントハンドラー群
    private void FilenamePasteButton_Click(object sender, RoutedEventArgs e)
    {
        if (Clipboard.ContainsText())
        {
            FilenameTextBox.Text = Clipboard.GetText();
            NormalizeFilenameTextBox();
        }
    }

    private void FilenameTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        NormalizeFilenameTextBox();
    }

    private void NormalizeFilenameTextBox()
    {
        FilenameTextBox.Text = FilenameTextBox.Text
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

    private void RecButton_Click(object sender, RoutedEventArgs e)
    {
        var filename = FilenameTextBox.Text.Trim();
        if (string.IsNullOrEmpty(filename))
        {
            filename = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            FilenameTextBox.Text = filename;
        }

        PvCtrlUtil.SetSubmitSaveAsDialog(filename);
        InvokePvMenu(["ファイル", "録画..."], $"ファイル名「{filename}」で録画開始しました．");
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        InvokePvMenu(["ファイル", "録画停止"], "録画停止しました．");
        StopReserveCheckBox.IsChecked = false;
    }

    private void Min25Button_Click(object sender, RoutedEventArgs e) => SetMinutesAndAlarm(25, 1);
    private void Min30Button_Click(object sender, RoutedEventArgs e) => SetMinutesAndAlarm(30, 2);
    private void Min60Button_Click(object sender, RoutedEventArgs e) => SetMinutesAndAlarm(60, 2);
    private void Min90Button_Click(object sender, RoutedEventArgs e) => SetMinutesAndAlarm(90, 2);
    private void Min120Button_Click(object sender, RoutedEventArgs e) => SetMinutesAndAlarm(120, 2);
    private void Min180Button_Click(object sender, RoutedEventArgs e) => SetMinutesAndAlarm(180, 2);

    private void SetMinutesAndAlarm(int minutes, int alarm)
    {
        MinUpDown.Text = minutes.ToString();
        AlarmUpDown.Text = alarm.ToString();
    }

    private void InvokePVButton_Click(object sender, RoutedEventArgs e)
    {
        PvCtrlUtil.InvokePv();
    }

    private void LineAButton_Click(object sender, RoutedEventArgs e)
    {
        InvokePvMenu(["設定", "映像・音声入力端子", "A"], "入力端子 A に切り替えました．");
    }

    private void LineBButton_Click(object sender, RoutedEventArgs e)
    {
        InvokePvMenu(["設定", "映像・音声入力端子", "B"], "入力端子 B に切り替えました．");
    }

    private void SoundOnButton_Click(object sender, RoutedEventArgs e)
    {
        InvokePvMenu(["設定", "モニタ時に音声を出力"], "音声 on/off を切り替えました．");
    }

    private void StopReserveCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
    {
        if (StopReserveCheckBox.IsChecked == true)
        {
            if (int.TryParse(MinUpDown.Text, out int minutes) &&
                int.TryParse(AlarmUpDown.Text, out int alarmMinutes))
            {
                var closePv = ClosePvReserveCheckBox.IsChecked == true;

                // タイマー開始時に時刻表示を更新
                var stopTime = DateTime.Now.AddMinutes(minutes);
                StopTimeLabel.Content = stopTime.ToString("HH:mm:ss");

                PvCtrlUtil.StartRecTimer(
                    minutes,
                    alarmMinutes,
                    _ =>
                    {
                        // 残り時間を更新
                        var remaining = stopTime - DateTime.Now;
                        if (remaining.TotalSeconds > 0)
                        {
                            var remainStr =
                                $"{(int)remaining.TotalHours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}";
                            Dispatcher.Invoke(() =>
                            {
                                RemainedTimeLabel.Content = remainStr;
                                // ウィンドウタイトルにも表示
                                Title = $"PvCtrl - 録画停止予約 {remainStr}";
                            });
                        }
                    },
                    recStop =>
                    {
                        // 停止時の処理
                        Dispatcher.Invoke(() =>
                        {
                            if (!recStop) return;

                            StopTimeLabel.Content = "00:00:00";
                            RemainedTimeLabel.Content = "00:00:00";
                            StopReserveCheckBox.IsChecked = false;
                            Title = "PvCtrl";
                            InvokePvMenu(["ファイル", "録画停止"], "予約により録画停止しました．");
                            if (closePv)
                            {
                                // PVを閉じる処理はPvCtrlUtilで処理される
                            }
                        });
                    }
                );
                ShowMessage($"{minutes}分後に録画停止予約を設定しました．");
            }
        }
        else
        {
            PvCtrlUtil.StopRecTimer(false);
            StopTimeLabel.Content = "00:00:00";
            RemainedTimeLabel.Content = "00:00:00";
            Title = "PvCtrl";
            ShowMessage("録画停止予約を解除しました．");
        }
    }

    private void ClosePvReserveCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
    {
        PvCtrlUtil.ClosePvReserve = ClosePvReserveCheckBox.IsChecked == true;
        var mode = ClosePvReserveCheckBox.IsChecked == true ? "セット" : "解除";
        ShowMessage($"PVクローズ予約を{mode}しました．");
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // ウィンドウの位置・サイズを復元
        try
        {
            var bounds = Settings.Default.Bounds;
            if (bounds is { Width: > 0, Height: > 0 })
            {
                Left = bounds.Left;
                Top = bounds.Top;
                Width = bounds.Width;
                Height = bounds.Height;
            }
            if (Settings.Default.WindowState == System.Windows.Forms.FormWindowState.Maximized)
            {
                WindowState = WindowState.Maximized;
            }
        }
        catch
        {
            // 設定読み込みエラーは無視
        }

        FilenameTextBox.Focus();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (StopReserveCheckBox.IsChecked == true)
        {
            PvCtrlUtil.StopRecTimer(false);
        }

        // ウィンドウの位置・サイズを保存
        try
        {
            if (WindowState == WindowState.Normal)
            {
                Settings.Default.Bounds = new System.Drawing.Rectangle(
                    (int)Left, (int)Top, (int)Width, (int)Height);
            }
            Settings.Default.WindowState = WindowState == WindowState.Maximized
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