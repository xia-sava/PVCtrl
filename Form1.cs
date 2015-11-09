using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace PVCtrl
{
    public partial class PvCtrl : Form
    {
        public PvCtrl()
        {
            InitializeComponent();
        }


        private bool InvokePVMenu(string[] menuItems, string message = "")
        {
            try
            {
                PvCtrlUtil.ControlMenu(menuItems);
                this.ShowMessage(message);
                return true;
            }
            catch
            {
                this.ErrorMessage();
                return false;
            }
        }

        private void SetMessage(string message)
        {
            this.MessageTextBox.Text = String.Format($"{DateTime.Now.ToString("hh:mm:ss")} {message}\r\n{this.MessageTextBox.Text}");
        }

        private void ShowMessage(string message)
        {
            System.Media.SystemSounds.Hand.Play();
            this.SetMessage(message);
        }

        private void ErrorMessage(string message = "何らかエラーが発生したっぽい？")
        {
            System.Media.SystemSounds.Beep.Play();
            this.SetMessage("Error: " + message);
        }


        private void FilenamePasteButton_Click(object sender, EventArgs e)
        {
            this.FilenameTextBox.Text = Clipboard.GetText();
        }

        private void RecButton_Click(object sender, EventArgs e)
        {
            var filename = this.FilenameTextBox.Text;
            if (filename.Length == 0)
            {
                filename = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                this.FilenameTextBox.Text = filename;
            }

            PvCtrlUtil.setSubmitSaveAsDialog(filename);

            this.InvokePVMenu(new[] { "ファイル", "録画..." }, $"ファイル名「{filename}」で録画開始しました．");
            //this.InvokePVMenu(new[] { "ファイル", "開く", "ファイル..." });
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            this.InvokePVMenu(new[] { "ファイル", "録画停止" }, "録画停止しました．");
            this.StopReserveCheckBox.Checked = false;
        }

        private void Min30Button_Click(object sender, EventArgs e)
        {
            MinUpDown.Value = 30;
        }

        private void Min60Button_Click(object sender, EventArgs e)
        {
            MinUpDown.Value = 60;
        }

        private void Min90Button_Click(object sender, EventArgs e)
        {
            MinUpDown.Value = 90;
        }

        private void Min120Button_Click(object sender, EventArgs e)
        {
            MinUpDown.Value = 120;
        }

        private void Min180Button_Click(object sender, EventArgs e)
        {
            MinUpDown.Value = 180;
        }

        private void StopReserveCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as CheckBox).Checked)
            {
                var minupdown = Math.Round(this.MinUpDown.Value);
                var alarmupdown = Math.Round(this.AlarmUpDown.Value);
                PvCtrlUtil.StartRecTimer(
                    (int)minupdown,
                    (int)alarmupdown,
                    (DateTime stopTime) =>
                    {
                        Invoke((MethodInvoker)(() => this.StopTimeLabel.Text = stopTime.ToString("HH:mm:ss")));
                        Invoke((MethodInvoker)(() => this.RemainedTimeLabel.Text = (stopTime - DateTime.Now).ToString(@"hh\:mm\:ss")));
                    },
                    (bool PVRecStop) =>
                    {
                        Invoke((MethodInvoker)(() =>
                        {
                            this.StopTimeLabel.Text = "00:00:00";
                            this.RemainedTimeLabel.Text = "00:00:00";
                            this.StopReserveCheckBox.Checked = false;
                            if (PVRecStop)
                            {
                                this.InvokePVMenu(new[] { "ファイル", "録画停止" }, "予約により録画停止しました．");
                            }
                        }));
                    });
                this.ShowMessage($"録画停止予約を{minupdown}分後に設定しました．");
            }
            else
            {
                PvCtrlUtil.StopRecTimer(false);
                this.ShowMessage("録画停止予約を解除しました．");
            }
        }

        private void InvokePVButton_Click(object sender, EventArgs e)
        {
            PvCtrlUtil.InvokePV();
        }

        private void LineAButton_Click(object sender, EventArgs e)
        {
            this.InvokePVMenu(new[] { "設定", "映像・音声入力端子", "A" }, "入力端子 A に切り替えました．");
        }

        private void LineBButton_Click(object sender, EventArgs e)
        {
            this.InvokePVMenu(new[] { "設定", "映像・音声入力端子", "B" }, "入力端子 B に切り替えました．");
        }

        private void SoundOnButton_Click(object sender, EventArgs e)
        {
            this.InvokePVMenu(new[] { "設定", "モニタ時に音声を出力" }, "音声 on/off を切り替えました．");
        }

        private void PvCtrl_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.StopReserveCheckBox.Checked)
            {
                PvCtrlUtil.StopRecTimer(false);
            }
        }
    }
}
