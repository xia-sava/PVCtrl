using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Windows.Automation;
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
                var pv = AutomationElement.FromHandle(PVProcess.MainWindowHandle);
                var menubar = PvCtrlUtil.getAutomationElement(pv, TreeScope.Children, ControlType.MenuBar, "アプリケーション");
                Thread.Sleep(100);

                var current = menubar;
                foreach (var name in menuItems.Take(menuItems.Count() - 1))
                {
                    var menuItem = PvCtrlUtil.getAutomationElement(current, TreeScope.Descendants, ControlType.MenuItem, name);
                    if (menuItem.Current.IsEnabled)
                    {
                        var menuPattern = menuItem.GetCurrentPattern(ExpandCollapsePattern.Pattern) as ExpandCollapsePattern;
                        menuPattern.Expand();
                        do
                        {
                            Thread.Sleep(100);
                        } while (menuPattern.Current.ExpandCollapseState == ExpandCollapseState.Collapsed);
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
            else
            {
                return false;
            }
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

                        var filenameBox = PvCtrlUtil.getAutomationElement(element, TreeScope.Descendants, ControlType.ComboBox, "ファイル名:");
                        (filenameBox.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern).SetValue(filename);
                        Thread.Sleep(100);

                        //var submitButton = PvCtrlUtil.getAutomationElement(element, TreeScope.Children, ControlType.Button, "開く(O)");
                        var submitButton = PvCtrlUtil.getAutomationElement(element, TreeScope.Children, ControlType.Button, "保存(S)");
                        (submitButton.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern).Invoke();
                    });
            }
        }

        static public AutomationElement getAutomationElement(AutomationElement parent, TreeScope scope, ControlType type, string name)
        {
            var element = parent.FindFirst(scope, new AndCondition(
                new PropertyCondition(AutomationElement.ControlTypeProperty, type),
                new PropertyCondition(AutomationElement.NameProperty, name),
                Automation.ControlViewCondition));

            return element;
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
