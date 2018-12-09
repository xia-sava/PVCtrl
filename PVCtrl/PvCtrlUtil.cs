using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Windows.Automation;
using SendKeys = System.Windows.Forms.SendKeys;
using Thread = System.Threading.Thread;
using System.Timers;
using System.Media;

namespace PVCtrl
{
    class PvCtrlUtil
    {
        static Timer recTimer;
        static Action<bool> stopHandler;

        static public bool CheckPVExists()
        {
            return (PvCtrlUtil.GetPVProcess() != null);
        }


        static public Process GetPVProcess()
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


        static public void InvokePV()
        {
            foreach (var filename in new[] { @"C:\Program Files\EARTH SOFT\PV\PV.exe", @"C:\Program Files (x86)\EARTH SOFT\PV\PV.exe" })
            {

                if (File.Exists(filename))
                {
                    Process.Start(filename);
                    return;
                }
            }
        }


        static public bool ControlMenu(string[] menuItems)
        {
            var PVProcess = PvCtrlUtil.GetPVProcess();
            if (PVProcess != null)
            {
                var retry = 10;
                while (retry-- > 0)
                {
                    try
                    {
                        var pv = AutomationElement.FromHandle(PVProcess.MainWindowHandle);
                        var menubar = PvCtrlUtil.getAutomationElement(pv, TreeScope.Children, ControlType.MenuBar, "アプリケーション");
                        Thread.Sleep(100);

                        var current = menubar;
                        foreach (var name in menuItems.Take(menuItems.Count() - 1))
                        {
                            var menuItem = PvCtrlUtil.getAutomationElement(current, TreeScope.Descendants, ControlType.MenuItem, name);
                            var menuPattern = menuItem.GetCurrentPattern(ExpandCollapsePattern.Pattern) as ExpandCollapsePattern;
                            menuPattern.Expand();
                            while (menuPattern.Current.ExpandCollapseState == ExpandCollapseState.Collapsed)
                            {
                                Thread.Sleep(100);
                            }
                            current = menuItem;
                        }
                        var command = PvCtrlUtil.getAutomationElement(current, TreeScope.Descendants, ControlType.MenuItem, menuItems.Last());
                        if (command.Current.IsEnabled)
                        {
                            var commandPattern = command.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                            commandPattern.Invoke();

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

        static public void setSubmitSaveAsDialog(string filename)
        {
            if (PvCtrlUtil.CheckPVExists())
            {
                Automation.AddAutomationEventHandler(
                    WindowPattern.WindowOpenedEvent,
                    AutomationElement.FromHandle(PvCtrlUtil.GetPVProcess().MainWindowHandle),
                    TreeScope.Children,
                    (sender, e) =>
                    {
                        var element = sender as AutomationElement;
                        if (element.Current.Name != "名前を付けて保存") return;
                        //if (element.Current.Name != "開く") return;

                        var filenameBox = PvCtrlUtil.getAutomationElement(element, TreeScope.Descendants, ControlType.Edit, "ファイル名:");
                        var filenameBoxValuePattern = filenameBox.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;
                        filenameBoxValuePattern.SetValue(filename);
                        while (filenameBoxValuePattern.Current.Value != filename)
                        {
                            Debug.WriteLine(filenameBoxValuePattern.Current.Value);
                            Thread.Sleep(100);
                        }

                        //var submitButton = PvCtrlUtil.getAutomationElement(element, TreeScope.Children, ControlType.Button, "開く(O)");
                        var submitButton = PvCtrlUtil.getAutomationElement(element, TreeScope.Children, ControlType.Button, "保存(S)");
                        (submitButton.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern).Invoke();
                        Automation.RemoveAllEventHandlers();
                    });
            }
        }

        static public AutomationElement getAutomationElement(AutomationElement parent, TreeScope scope, ControlType type, string name)
        {
            return parent.FindFirst(scope, new AndCondition(
                new PropertyCondition(AutomationElement.ControlTypeProperty, type),
                new PropertyCondition(AutomationElement.NameProperty, name),
                new PropertyCondition(AutomationElement.IsEnabledProperty, true),
                Automation.ControlViewCondition));
        }

        static public void StartRecTimer(int minutes, int alarmMinute, Action<DateTime> elapsedHandler, Action<bool> stopHandler)
        {
            var alarmed = (alarmMinute == 0);
            var stopTime = DateTime.Now.AddMinutes(minutes);
            PvCtrlUtil.stopHandler = stopHandler;
            PvCtrlUtil.recTimer = new Timer(1000);
            PvCtrlUtil.recTimer.Elapsed += (object sender, ElapsedEventArgs e) =>
            {
                elapsedHandler(stopTime);
                if (stopTime.AddMinutes(-alarmMinute) < DateTime.Now)
                {
                    if (!alarmed)
                    {
                        new SoundPlayer(Properties.Resources.TimeStopSound).Play();
                        alarmed = true;
                    }
                }
                if (stopTime < DateTime.Now)
                {
                    PvCtrlUtil.StopRecTimer(true);
                }
            };
            PvCtrlUtil.recTimer.Start();
        }

        static public void StopRecTimer(bool recStop)
        {
            if (PvCtrlUtil.recTimer != null && PvCtrlUtil.recTimer.Enabled)
            {
                PvCtrlUtil.recTimer.Stop();
                if (PvCtrlUtil.stopHandler != null)
                {
                    PvCtrlUtil.stopHandler(recStop);
                }
            }
        }
    }
}
