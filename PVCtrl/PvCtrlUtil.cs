using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;
using PVCtrl.Properties;
using Timer = System.Timers.Timer;

namespace PVCtrl
{
    static class PvCtrlUtil
    {
        private static Timer _recTimer;
        private static Action<bool> _recStopHandler;

        // ReSharper disable once MemberCanBePrivate.Global
        public static bool CheckPvExists()
        {
            return (GetPvProcess() != null);
        }


        // ReSharper disable once MemberCanBePrivate.Global
        public static Process GetPvProcess()
        {
            foreach (Process p in Process.GetProcesses())
            {
                if (p.ProcessName == "PV")
                {
                    return p;
                }
            }
            return null;
        }


        public static void InvokePv()
        {
            foreach (var filename in new[] { @"C:\Program Files\EARTH SOFT\PV\PV.exe", @"C:\Program Files (x86)\EARTH SOFT\PV\PV.exe" })
            {

                if (File.Exists(filename))
                {
                    Task.Run(async () =>
                    {
                        var proc = Process.Start(filename);
                        if (proc != null)
                        {
                            await Task.Delay(3000);
                            proc.PriorityClass = ProcessPriorityClass.High;
                        }
                    });
                    return;
                }
            }
        }


        // ReSharper disable once UnusedMethodReturnValue.Global
        public static bool ControlMenu(string[] menuItems)
        {
            var pvProcess = GetPvProcess();
            if (pvProcess != null)
            {
                var retry = 10;
                while (retry-- > 0)
                {
                    try
                    {
                        var pv = AutomationElement.FromHandle(pvProcess.MainWindowHandle);
                        var menubar = GetAutomationElement(pv, TreeScope.Children, ControlType.MenuBar, "アプリケーション");
                        Thread.Sleep(100);

                        var current = menubar;
                        foreach (var name in menuItems.Take(menuItems.Count() - 1))
                        {
                            var menuItem = GetAutomationElement(current, TreeScope.Descendants, ControlType.MenuItem, name);
                            var menuPattern = menuItem.GetCurrentPattern(ExpandCollapsePattern.Pattern) as ExpandCollapsePattern;
                            menuPattern?.Expand();
                            while (menuPattern?.Current.ExpandCollapseState == ExpandCollapseState.Collapsed)
                            {
                                Thread.Sleep(100);
                            }
                            current = menuItem;
                        }
                        var command = GetAutomationElement(current, TreeScope.Descendants, ControlType.MenuItem, menuItems.Last());
                        if (command.Current.IsEnabled)
                        {
                            var commandPattern = command.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                            commandPattern?.Invoke();

                        }

                        return true;
                    }
                    catch
                    {
                        // Automation はなぜか時々 Exception を吐くので，その時はメニューを初期状態に戻してリトライする．
                        SendKeys.SendWait("{ESC}{ESC}{ESC}{ESC}{ESC}");
                    }
                }
            }
            return false;
        }

        public static void SetSubmitSaveAsDialog(string filename)
        {
            if (CheckPvExists())
            {
                Automation.AddAutomationEventHandler(
                    WindowPattern.WindowOpenedEvent,
                    AutomationElement.FromHandle(GetPvProcess().MainWindowHandle),
                    TreeScope.Children,
                    (sender, e) =>
                    {
                        var element = (AutomationElement)sender;
                        if (element.Current.Name != "名前を付けて保存") return;
                        //if (element.Current.Name != "開く") return;

                        var filenameBox = PvCtrlUtil.GetAutomationElement(element, TreeScope.Descendants, ControlType.Edit, "ファイル名:");
                        var filenameBoxValuePattern = (ValuePattern)filenameBox.GetCurrentPattern(ValuePattern.Pattern);
                        filenameBoxValuePattern.SetValue(filename);
                        while (filenameBoxValuePattern.Current.Value != filename)
                        {
                            Debug.WriteLine(filenameBoxValuePattern.Current.Value);
                            Thread.Sleep(100);
                        }
                    });
            }
        }

        private static AutomationElement GetAutomationElement(AutomationElement parent, TreeScope scope, ControlType type, string name)
        {
            return parent.FindFirst(scope, new AndCondition(
                new PropertyCondition(AutomationElement.ControlTypeProperty, type),
                new PropertyCondition(AutomationElement.NameProperty, name),
                new PropertyCondition(AutomationElement.IsEnabledProperty, true),
                Automation.ControlViewCondition));
        }

        public static void StartRecTimer(int minutes, int alarmMinute, Action<DateTime> elapsedHandler, Action<bool> stopHandler)
        {
            var alarmed = (alarmMinute == 0);
            var stopTime = DateTime.Now.AddMinutes(minutes);
            _recStopHandler = stopHandler;
            _recTimer = new Timer(1000);
            _recTimer.Elapsed += (sender, e) =>
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
        }
    }
}
