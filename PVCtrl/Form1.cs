using System;
using System.Media;
using System.Windows.Forms;
using PVCtrl.Properties;

namespace PVCtrl
{
    public partial class PvCtrl : Form
    {
        public PvCtrl()
        {
            InitializeComponent();
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool InvokePvMenu(string[] menuItems, string message = "")
        {
            try
            {
                PvCtrlUtil.ControlMenu(menuItems);
                ShowMessage(message);
                return true;
            }
            catch (Exception)
            {
                ErrorMessage();
                return false;
            }
        }

        private void SetMessage(string message)
        {
            MessageTextBox.Text =
                String.Format($"{DateTime.Now:hh:mm:ss} {message}\r\n{MessageTextBox.Text}");
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


        private void FilenamePasteButton_Click(object sender, EventArgs e)
        {
            FilenameTextBox.Text = Clipboard.GetText();
            NormalizeFilenameTextBox();
        }


        private void FilenameTextBox_Leave(object sender, EventArgs e)
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
                    .Replace('"', '”')
                    .Replace('\'', '’')
                    .Replace('<', '＜')
                    .Replace('>', '＞')
                    .Replace('|', '｜')
                    .Replace('　', ' ')
                    .Replace("\n", "")
                    .Replace("\r", "")
                ;
        }


        private void RecButton_Click(object sender, EventArgs e)
        {
            var filename = FilenameTextBox.Text;
            if (filename.Length == 0)
            {
                filename = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                FilenameTextBox.Text = filename;
            }

            PvCtrlUtil.SetSubmitSaveAsDialog(filename);

            InvokePvMenu(new[] {"ファイル", "録画..."}, $"ファイル名「{filename}」で録画開始しました．");
            //this.InvokePVMenu(new[] { "ファイル", "開く", "ファイル..." });
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            InvokePvMenu(new[] {"ファイル", "録画停止"}, "録画停止しました．");
            StopReserveCheckBox.Checked = false;
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
            if (sender is CheckBox checkBox && checkBox.Checked)
            {
                var minupdown = Math.Round(MinUpDown.Value);
                var alarmupdown = Math.Round(AlarmUpDown.Value);
                PvCtrlUtil.StartRecTimer(
                    (int) minupdown,
                    (int) alarmupdown,
                    stopTime =>
                    {
                        Invoke((MethodInvoker) (() => StopTimeLabel.Text = $"{stopTime:HH:mm:ss}"));
                        Invoke((MethodInvoker) (() =>
                            RemainedTimeLabel.Text = $"{(stopTime - DateTime.Now):hh\\:mm\\:ss}"));
                        Invoke((MethodInvoker) (() =>
                            Text = $"PvCtrl - 録画停止予約 {(stopTime - DateTime.Now):hh\\:mm\\:ss}"));
                    },
                    pvRecStop =>
                    {
                        Invoke((MethodInvoker) (() =>
                        {
                            StopTimeLabel.Text = "00:00:00";
                            RemainedTimeLabel.Text = "00:00:00";
                            StopReserveCheckBox.Checked = false;
                            if (pvRecStop)
                            {
                                InvokePvMenu(new[] {"ファイル", "録画停止"}, "予約により録画停止しました．");
                                Invoke((MethodInvoker) (() => Text = "PvCtrl"));
                            }
                        }));
                    });
                ShowMessage($"録画停止予約を{minupdown}分後に設定しました．");
            }
            else
            {
                PvCtrlUtil.StopRecTimer(false);
                ShowMessage("録画停止予約を解除しました．");
                Text = "PvCtrl";
            }
        }

        private void InvokePVButton_Click(object sender, EventArgs e)
        {
            PvCtrlUtil.InvokePv();
        }

        private void LineAButton_Click(object sender, EventArgs e)
        {
            InvokePvMenu(new[] {"設定", "映像・音声入力端子", "A"}, "入力端子 A に切り替えました．");
        }

        private void LineBButton_Click(object sender, EventArgs e)
        {
            InvokePvMenu(new[] {"設定", "映像・音声入力端子", "B"}, "入力端子 B に切り替えました．");
        }

        private void SoundOnButton_Click(object sender, EventArgs e)
        {
            InvokePvMenu(new[] {"設定", "モニタ時に音声を出力"}, "音声 on/off を切り替えました．");
        }

        private void PvCtrl_Load(object sender, EventArgs e)
        {
            // ウィンドウの位置・サイズを復元
            Bounds = Settings.Default.Bounds;
            WindowState = Settings.Default.WindowState;
            FilenameTextBox.Focus();
        }

        private void PvCtrl_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (StopReserveCheckBox.Checked)
            {
                PvCtrlUtil.StopRecTimer(false);
            }

            // ウィンドウの位置・サイズを保存
            Settings.Default.Bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;

            Settings.Default.WindowState = WindowState;

            Settings.Default.Save();
        }

        private void ClosePvReserveCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                PvCtrlUtil.ClosePvReserve = checkBox.Checked;
                var mode = checkBox.Checked ? "セット" : "解除";
                ShowMessage($"PVクローズ予約を{mode}しました．");
            }
        }
    }
}