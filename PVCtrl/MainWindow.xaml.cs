using System;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PVCtrl;

[SupportedOSPlatform("windows6.1")]
public partial class MainWindow
{
    private MainViewModel ViewModel => (MainViewModel)DataContext;

    public MainWindow()
    {
        InitializeComponent();
        SetupNumericTextBoxes();
    }

    private void SetupNumericTextBoxes()
    {
        // NumericUpDown代替のTextBoxに数値のみ入力を許可
        var minTextBox = FindName("MinUpDown") as TextBox;
        var alarmTextBox = FindName("AlarmUpDown") as TextBox;

        if (minTextBox != null)
            minTextBox.PreviewTextInput += NumericTextBox_PreviewTextInput;
        if (alarmTextBox != null)
            alarmTextBox.PreviewTextInput += NumericTextBox_PreviewTextInput;
    }

    private static void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // 数字のみ許可
        e.Handled = !int.TryParse(e.Text, out _);
    }

    private void NumericUpDown_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not TextBox textBox) return;
        if (!int.TryParse(textBox.Text, out var value)) return;

        var newValue = e.Delta > 0 ? value + 1 : value - 1;

        // 範囲チェック
        if (textBox.Name == "MinUpDown" || ReferenceEquals(textBox, FindName("MinUpDown")))
        {
            newValue = Math.Max(1, Math.Min(1440, newValue));
            ViewModel.MinUpDown = newValue.ToString();
        }
        else if (textBox.Name == "AlarmUpDown" || ReferenceEquals(textBox, FindName("AlarmUpDown")))
        {
            newValue = Math.Max(0, Math.Min(1440, newValue));
            ViewModel.AlarmUpDown = newValue.ToString();
        }

        e.Handled = true;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.OnWindowLoaded();

        // フォーカス設定
        (FindName("FilenameTextBox") as TextBox)?.Focus();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        ViewModel.OnWindowClosing();
    }
}