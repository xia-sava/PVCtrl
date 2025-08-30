using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;
using PVCtrl.Properties;
using Timer = System.Timers.Timer;

namespace PVCtrl;

[SupportedOSPlatform("windows6.1")]
static class PvCtrlUtil
{
    private static Timer? _recTimer;
    private static Action<bool>? _recStopHandler;

    public static bool ClosePvReserve { set; get; }
    public static Action? OnPvClosed { set; get; }

    private static Process? GetPvProcess()
    {
        return Process.GetProcesses()
            .FirstOrDefault(p => p.ProcessName == "PV");
    }

    private static WindowVisualState GetWindowVisualState(this Process process)
    {
        var element = AutomationElement.FromHandle(process.MainWindowHandle);
        var windowPattern = (WindowPattern)element.GetCurrentPattern(WindowPattern.Pattern);
        return windowPattern.Current.WindowVisualState;
    }

    private static void SetWindowVisualState(this Process process, WindowVisualState state)
    {
        var element = AutomationElement.FromHandle(process.MainWindowHandle);
        var windowPattern = (WindowPattern)element.GetCurrentPattern(WindowPattern.Pattern);
        windowPattern.SetWindowVisualState(state);
    }

    private static void RestoreWindow(this Process process)
    {
        process.SetWindowVisualState(WindowVisualState.Normal);
    }

    private static void MinimizeWindow(this Process process)
    {
        process.SetWindowVisualState(WindowVisualState.Minimized);
    }


    private static Process? GetExistsPv()
    {
        var process = GetPvProcess();
        process?.RestoreWindow();
        return process;
    }


    public static void InvokePv()
    {
        if (GetPvProcess() is { } process)
        {
            if (process.GetWindowVisualState() == WindowVisualState.Minimized)
            {
                process.RestoreWindow();
            }
            else
            {
                process.MinimizeWindow();
            }
        }
        else
        {
            foreach (var filename in new[]
                     {
                         @"C:\Program Files\EARTH SOFT\PV\PV.exe",
                         @"C:\Program Files (x86)\EARTH SOFT\PV\PV.exe"
                     }
                    )
            {
                if (File.Exists(filename))
                {
                    Task.Run(async () =>
                    {
                        if (Process.Start(filename) is { } proc)
                        {
                            await Task.Delay(3000);
                            proc.PriorityClass = ProcessPriorityClass.High;
                        }
                    });
                    return;
                }
            }
        }
    }


    public static void ControlMenu(string[] menuItems)
    {
        if (GetExistsPv() is not { } pvProcess) return;
        var retry = 10;
        while (retry-- > 0)
        {
            try
            {
                var pv = AutomationElement.FromHandle(pvProcess.MainWindowHandle);
                var menubar = GetAutomationElement(pv, TreeScope.Children, ControlType.MenuBar, "アプリケーション");
                Thread.Sleep(100);

                var current = menubar;
                foreach (var name in menuItems.Take(menuItems.Length - 1))
                {
                    var menuItem = GetAutomationElement(current, TreeScope.Descendants, ControlType.MenuItem,
                        name);
                    var menuPattern =
                        menuItem.GetCurrentPattern(ExpandCollapsePattern.Pattern) as ExpandCollapsePattern;
                    menuPattern?.Expand();
                    while (menuPattern?.Current.ExpandCollapseState == ExpandCollapseState.Collapsed)
                    {
                        Thread.Sleep(100);
                    }

                    current = menuItem;
                }

                var command = GetAutomationElement(current, TreeScope.Descendants, ControlType.MenuItem,
                    menuItems.Last());
                if (command.Current.IsEnabled)
                {
                    var commandPattern = command.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                    commandPattern?.Invoke();
                }

                return;
            }
            catch
            {
                // Automation はなぜか時々 Exception を吐くので，その時はメニューを初期状態に戻してリトライする．
                SendKeys.SendWait("{ESC}{ESC}{ESC}{ESC}{ESC}");
            }
        }
    }

    public static void SetSubmitSaveAsDialog(string filename)
    {
        var process = GetExistsPv();
        if (process != null)
        {
            Automation.AddAutomationEventHandler(
                WindowPattern.WindowOpenedEvent,
                AutomationElement.FromHandle(process.MainWindowHandle),
                TreeScope.Children,
                (sender, _) =>
                {
                    var element = (AutomationElement)sender;
                    if (element.Current.Name != "名前を付けて保存") return;

                    var filenameBox = GetAutomationElement(element, TreeScope.Descendants,
                        ControlType.Edit, "ファイル名:");
                    var filenameBoxValuePattern = (ValuePattern)filenameBox.GetCurrentPattern(ValuePattern.Pattern);
                    filenameBoxValuePattern.SetValue(filename);
                    while (filenameBoxValuePattern.Current.Value != filename)
                    {
                        Thread.Sleep(100);
                    }

                    var submitButton =
                        GetAutomationElement(element, TreeScope.Children, ControlType.Button, "保存(S)");
                    ((InvokePattern)submitButton.GetCurrentPattern(InvokePattern.Pattern)).Invoke();
                    Automation.RemoveAllEventHandlers();
                });
        }
    }

    private static AutomationElement GetAutomationElement(AutomationElement parent, TreeScope scope,
        ControlType type, string name)
    {
        return parent.FindFirst(scope, new AndCondition(
            new PropertyCondition(AutomationElement.ControlTypeProperty, type),
            new PropertyCondition(AutomationElement.NameProperty, name),
            new PropertyCondition(AutomationElement.IsEnabledProperty, true),
            Automation.ControlViewCondition));
    }

    public static void StartRecTimer(int minutes, int alarmMinute, Action<DateTime> elapsedHandler,
        Action<bool> stopHandler)
    {
        var alarmed = alarmMinute == 0;
        var stopTime = DateTime.Now.AddMinutes(minutes);
        _recStopHandler = stopHandler;
        _recTimer = new Timer(1000);
        _recTimer.Elapsed += (_, _) =>
        {
            elapsedHandler(stopTime);
            if (stopTime.AddMinutes(-alarmMinute) < DateTime.Now)
            {
                if (!alarmed)
                {
                    new SoundPlayer(Resources.TimeStopSound).Play();
                    alarmed = true;
                }
            }

            if (stopTime < DateTime.Now)
            {
                StopRecTimer(true);
            }
        };
        _recTimer.Start();
    }

    public static void StopRecTimer(bool recStop)
    {
        if (_recTimer?.Enabled != true) return;
        _recTimer.Stop();
        _recStopHandler?.Invoke(recStop);
        if (recStop && ClosePvReserve)
        {
            ControlMenu(["ファイル", "終了"]);
            OnPvClosed?.Invoke();
        }
    }
}