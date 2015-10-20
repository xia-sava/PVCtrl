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


        private bool InvokePVMenu(string[] menuItems)
        {
            try
            {
                PvCtrlUtil.ControlMenu(menuItems);
                this.ClearMessage();
                return true;
            }
            catch
            {
                this.ErrorMessage();
                return false;
            }
        }

        private void ClearMessage()
        {
            this.MessageLabel.Text = "";
        }

        private void ErrorMessage(string message = "何らかエラーが発生したっぽい？")
        {
            System.Media.SystemSounds.Beep.Play();
            this.MessageLabel.Text = message;
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

            this.InvokePVMenu(new[] { "ファイル", "録画..." });
            //this.InvokePVMenu(new[] { "ファイル", "開く", "ファイル..." });
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            this.InvokePVMenu(new[] { "ファイル", "録画停止" });
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
                this.ClearMessage();
                PvCtrlUtil.StartRecTimer(
                    (int)this.MinUpDown.Value,
                    (int)this.AlarmUpDown.Value,
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
                                this.InvokePVMenu(new[] { "ファイル", "録画停止" });
                            }
                        }));
                    });
            }
            else
            {
                PvCtrlUtil.StopRecTimer(false);
                this.ClearMessage();
            }
        }

        private void InvokePVButton_Click(object sender, EventArgs e)
        {
            PvCtrlUtil.InvokePV();
        }

        private void LineAButton_Click(object sender, EventArgs e)
        {
            this.InvokePVMenu(new[] { "設定", "映像・音声入力端子", "A" });
        }

        private void LineBButton_Click(object sender, EventArgs e)
        {
            this.InvokePVMenu(new[] { "設定", "映像・音声入力端子", "B" });
        }

        private void SoundOnButton_Click(object sender, EventArgs e)
        {
            this.InvokePVMenu(new[] { "設定", "モニタ時に音声を出力" });
        }
    }
}
