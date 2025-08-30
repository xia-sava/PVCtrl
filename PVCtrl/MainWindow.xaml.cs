using System;
using System.Runtime.Versioning;
using System.Windows;
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
        RecordMinutesTextBox.PreviewTextInput += NumericTextBox_PreviewTextInput;
        AlarmMinutesTextBox.PreviewTextInput += NumericTextBox_PreviewTextInput;
    }

    private static void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !int.TryParse(e.Text, out _);
    }

    private void RecordMinutesTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        HandleNumericMouseWheel(e, delta => ViewModel.AdjustRecordMinutes(delta));
    }

    private void AlarmMinutesTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        HandleNumericMouseWheel(e, delta => ViewModel.AdjustAlarmMinutes(delta));
    }

    private static void HandleNumericMouseWheel(MouseWheelEventArgs e, Action<int> adjustAction)
    {
        var delta = e.Delta > 0 ? 1 : -1;
        adjustAction(delta);
        e.Handled = true;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.OnWindowLoaded();
        FilenameTextBox?.Focus();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        ViewModel.OnWindowClosing();
    }
}
