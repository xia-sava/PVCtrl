using System.Windows.Forms;

namespace PVCtrl
{
    partial class PvCtrl
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PvCtrl));
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.FilenameCaptionLabel = new System.Windows.Forms.Label();
            this.FilenameTextBox = new System.Windows.Forms.TextBox();
            this.FilenamePasteButton = new System.Windows.Forms.Button();
            this.Filler1Label = new System.Windows.Forms.Label();
            this.RecButton = new System.Windows.Forms.Button();
            this.StopButton = new System.Windows.Forms.Button();
            this.flowLayoutPanel7 = new System.Windows.Forms.FlowLayoutPanel();
            this.InvokePVButton = new System.Windows.Forms.Button();
            this.LineCaptionLabel = new System.Windows.Forms.Label();
            this.LineAButton = new System.Windows.Forms.Button();
            this.LineBButton = new System.Windows.Forms.Button();
            this.SoundCaptionLabel = new System.Windows.Forms.Label();
            this.SoundOnButton = new System.Windows.Forms.Button();
            this.AlarmCaptionLabel = new System.Windows.Forms.Label();
            this.AlarmUpDown = new PVCtrl.WheelableNumericUpDown();
            this.AlarmCaption2Label = new System.Windows.Forms.Label();
            this.flowLayoutPanel10 = new System.Windows.Forms.FlowLayoutPanel();
            this.Min30Button = new System.Windows.Forms.Button();
            this.Min60Button = new System.Windows.Forms.Button();
            this.Min90Button = new System.Windows.Forms.Button();
            this.Min120Button = new System.Windows.Forms.Button();
            this.Min180Button = new System.Windows.Forms.Button();
            this.MinUpDown = new PVCtrl.WheelableNumericUpDown();
            this.StopReserveMinLabel = new System.Windows.Forms.Label();
            this.StopReserveCheckBox = new System.Windows.Forms.CheckBox();
            this.StopTimeCaptionLabel = new System.Windows.Forms.Label();
            this.RemainedTimeCaptionLabel = new System.Windows.Forms.Label();
            this.StopTimeLabel = new System.Windows.Forms.Label();
            this.RemainedTimeLabel = new System.Windows.Forms.Label();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.MessageTextBox = new System.Windows.Forms.TextBox();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AlarmUpDown)).BeginInit();
            this.flowLayoutPanel10.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MinUpDown)).BeginInit();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            //
            // flowLayoutPanel1
            //
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.Controls.Add(this.FilenameCaptionLabel);
            this.flowLayoutPanel1.Controls.Add(this.FilenameTextBox);
            this.flowLayoutPanel1.Controls.Add(this.FilenamePasteButton);
            this.flowLayoutPanel1.Controls.Add(this.Filler1Label);
            this.flowLayoutPanel1.Controls.Add(this.RecButton);
            this.flowLayoutPanel1.Controls.Add(this.StopButton);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1182, 55);
            this.flowLayoutPanel1.TabIndex = 0;
            //
            // FilenameCaptionLabel
            //
            this.FilenameCaptionLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.FilenameCaptionLabel.AutoSize = true;
            this.FilenameCaptionLabel.Location = new System.Drawing.Point(3, 10);
            this.FilenameCaptionLabel.Name = "FilenameCaptionLabel";
            this.FilenameCaptionLabel.Size = new System.Drawing.Size(182, 31);
            this.FilenameCaptionLabel.TabIndex = 0;
            this.FilenameCaptionLabel.Text = "録画ファイル名";
            //
            // FilenameTextBox
            //
            this.FilenameTextBox.Location = new System.Drawing.Point(191, 3);
            this.FilenameTextBox.Name = "FilenameTextBox";
            this.FilenameTextBox.Size = new System.Drawing.Size(659, 46);
            this.FilenameTextBox.TabIndex = 1;
            this.FilenameTextBox.Leave += new System.EventHandler(this.FilenameTextBox_Leave);
            //
            // FilenamePasteButton
            //
            this.FilenamePasteButton.Location = new System.Drawing.Point(856, 3);
            this.FilenamePasteButton.Name = "FilenamePasteButton";
            this.FilenamePasteButton.Size = new System.Drawing.Size(100, 46);
            this.FilenamePasteButton.TabIndex = 2;
            this.FilenamePasteButton.Text = "paste";
            this.FilenamePasteButton.UseVisualStyleBackColor = true;
            this.FilenamePasteButton.Click += new System.EventHandler(this.FilenamePasteButton_Click);
            //
            // Filler1Label
            //
            this.Filler1Label.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Filler1Label.AutoSize = true;
            this.Filler1Label.Location = new System.Drawing.Point(962, 10);
            this.Filler1Label.Name = "Filler1Label";
            this.Filler1Label.Size = new System.Drawing.Size(21, 31);
            this.Filler1Label.TabIndex = 0;
            this.Filler1Label.Text = "";
            //
            // RecButton
            //
            this.RecButton.ForeColor = System.Drawing.Color.Red;
            this.RecButton.Location = new System.Drawing.Point(989, 3);
            this.RecButton.Name = "RecButton";
            this.RecButton.Size = new System.Drawing.Size(46, 46);
            this.RecButton.TabIndex = 3;
            this.RecButton.Tag = "require_pv";
            this.RecButton.Text = "●";
            this.RecButton.UseVisualStyleBackColor = true;
            this.RecButton.Click += new System.EventHandler(this.RecButton_Click);
            //
            // StopButton
            //
            this.StopButton.ForeColor = System.Drawing.Color.Blue;
            this.StopButton.Location = new System.Drawing.Point(1041, 3);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(46, 46);
            this.StopButton.TabIndex = 4;
            this.StopButton.Tag = "require_pv";
            this.StopButton.Text = "■";
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
            //
            // flowLayoutPanel10
            //
            this.flowLayoutPanel10.Controls.Add(this.Min30Button);
            this.flowLayoutPanel10.Controls.Add(this.Min60Button);
            this.flowLayoutPanel10.Controls.Add(this.Min90Button);
            this.flowLayoutPanel10.Controls.Add(this.Min120Button);
            this.flowLayoutPanel10.Controls.Add(this.Min180Button);
            this.flowLayoutPanel10.Controls.Add(this.MinUpDown);
            this.flowLayoutPanel10.Controls.Add(this.StopReserveMinLabel);
            this.flowLayoutPanel10.Controls.Add(this.StopReserveCheckBox);
            this.flowLayoutPanel10.Location = new System.Drawing.Point(12, 70);
            this.flowLayoutPanel10.Name = "flowLayoutPanel10";
            this.flowLayoutPanel10.Size = new System.Drawing.Size(876, 55);
            this.flowLayoutPanel10.TabIndex = 0;
            //
            // Min30Button
            //
            this.Min30Button.Location = new System.Drawing.Point(3, 3);
            this.Min30Button.Name = "Min30Button";
            this.Min30Button.Size = new System.Drawing.Size(80, 46);
            this.Min30Button.TabIndex = 6;
            this.Min30Button.Text = "30";
            this.Min30Button.UseVisualStyleBackColor = true;
            this.Min30Button.Click += new System.EventHandler(this.Min30Button_Click);
            //
            // Min60Button
            //
            this.Min60Button.Location = new System.Drawing.Point(89, 3);
            this.Min60Button.Name = "Min60Button";
            this.Min60Button.Size = new System.Drawing.Size(80, 46);
            this.Min60Button.TabIndex = 7;
            this.Min60Button.Text = "60";
            this.Min60Button.UseVisualStyleBackColor = true;
            this.Min60Button.Click += new System.EventHandler(this.Min60Button_Click);
            //
            // Min90Button
            //
            this.Min90Button.Location = new System.Drawing.Point(175, 3);
            this.Min90Button.Name = "Min90Button";
            this.Min90Button.Size = new System.Drawing.Size(80, 46);
            this.Min90Button.TabIndex = 8;
            this.Min90Button.Text = "90";
            this.Min90Button.UseVisualStyleBackColor = true;
            this.Min90Button.Click += new System.EventHandler(this.Min90Button_Click);
            //
            // Min120Button
            //
            this.Min120Button.Location = new System.Drawing.Point(261, 3);
            this.Min120Button.Name = "Min120Button";
            this.Min120Button.Size = new System.Drawing.Size(80, 46);
            this.Min120Button.TabIndex = 9;
            this.Min120Button.Text = "120";
            this.Min120Button.UseVisualStyleBackColor = true;
            this.Min120Button.Click += new System.EventHandler(this.Min120Button_Click);
            //
            // Min180Button
            //
            this.Min180Button.Location = new System.Drawing.Point(347, 3);
            this.Min180Button.Name = "Min180Button";
            this.Min180Button.Size = new System.Drawing.Size(80, 46);
            this.Min180Button.TabIndex = 10;
            this.Min180Button.Text = "180";
            this.Min180Button.UseVisualStyleBackColor = true;
            this.Min180Button.Click += new System.EventHandler(this.Min180Button_Click);
            //
            // MinUpDown
            //
            this.MinUpDown.Location = new System.Drawing.Point(433, 3);
            this.MinUpDown.Maximum = new decimal(new int[] {1440, 0, 0, 0});
            this.MinUpDown.Minimum = new decimal(new int[] {1, 0, 0, 0});
            this.MinUpDown.Name = "MinUpDown";
            this.MinUpDown.Size = new System.Drawing.Size(100, 46);
            this.MinUpDown.TabIndex = 11;
            this.MinUpDown.Tag = 30;
            this.MinUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.MinUpDown.Value = new decimal(new int[] {30, 0, 0, 0});
            //
            // StopReserveMinLabel
            //
            this.StopReserveMinLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.StopReserveMinLabel.AutoSize = true;
            this.StopReserveMinLabel.Location = new System.Drawing.Point(539, 10);
            this.StopReserveMinLabel.Name = "StopReserveMinLabel";
            this.StopReserveMinLabel.Size = new System.Drawing.Size(62, 31);
            this.StopReserveMinLabel.TabIndex = 0;
            this.StopReserveMinLabel.Text = "分後";
            //
            // StopReserveCheckBox
            //
            this.StopReserveCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
            this.StopReserveCheckBox.AutoSize = true;
            this.StopReserveCheckBox.Font = new System.Drawing.Font("游ゴシック", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.StopReserveCheckBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.StopReserveCheckBox.Location = new System.Drawing.Point(607, 3);
            this.StopReserveCheckBox.Name = "StopReserveCheckBox";
            this.StopReserveCheckBox.Size = new System.Drawing.Size(187, 45);
            this.StopReserveCheckBox.TabIndex = 12;
            this.StopReserveCheckBox.Text = "録画停止予約";
            this.StopReserveCheckBox.UseVisualStyleBackColor = true;
            this.StopReserveCheckBox.CheckedChanged += new System.EventHandler(this.StopReserveCheckBox_CheckedChanged);
            //
            // StopTimeCaptionLabel
            //
            this.StopTimeCaptionLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.StopTimeCaptionLabel.AutoSize = true;
            this.StopTimeCaptionLabel.Location = new System.Drawing.Point(3, 2);
            this.StopTimeCaptionLabel.Name = "StopTimeCaptionLabel";
            this.StopTimeCaptionLabel.Size = new System.Drawing.Size(158, 31);
            this.StopTimeCaptionLabel.TabIndex = 0;
            this.StopTimeCaptionLabel.Text = "録画停止時刻";
            //
            // RemainedTimeCaptionLabel
            //
            this.RemainedTimeCaptionLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.RemainedTimeCaptionLabel.AutoSize = true;
            this.RemainedTimeCaptionLabel.Location = new System.Drawing.Point(3, 37);
            this.RemainedTimeCaptionLabel.Name = "RemainedTimeCaptionLabel";
            this.RemainedTimeCaptionLabel.Size = new System.Drawing.Size(158, 31);
            this.RemainedTimeCaptionLabel.TabIndex = 0;
            this.RemainedTimeCaptionLabel.Text = "録画停止まで";
            //
            // StopTimeLabel
            //
            this.StopTimeLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.StopTimeLabel.AutoSize = true;
            this.flowLayoutPanel2.SetFlowBreak(this.StopTimeLabel, true);
            this.StopTimeLabel.Font = new System.Drawing.Font("游ゴシック", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.StopTimeLabel.Location = new System.Drawing.Point(167, 0);
            this.StopTimeLabel.Name = "StopTimeLabel";
            this.StopTimeLabel.Size = new System.Drawing.Size(121, 35);
            this.StopTimeLabel.TabIndex = 0;
            this.StopTimeLabel.Text = "00:00:00";
            //
            // RemainedTimeLabel
            //
            this.RemainedTimeLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.RemainedTimeLabel.AutoSize = true;
            this.RemainedTimeLabel.Font = new System.Drawing.Font("游ゴシック", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.RemainedTimeLabel.Location = new System.Drawing.Point(167, 35);
            this.RemainedTimeLabel.Name = "RemainedTimeLabel";
            this.RemainedTimeLabel.Size = new System.Drawing.Size(121, 35);
            this.RemainedTimeLabel.TabIndex = 0;
            this.RemainedTimeLabel.Text = "00:00:00";
            //
            // flowLayoutPanel7
            //
            this.flowLayoutPanel7.Controls.Add(this.InvokePVButton);
            this.flowLayoutPanel7.Controls.Add(this.LineCaptionLabel);
            this.flowLayoutPanel7.Controls.Add(this.LineAButton);
            this.flowLayoutPanel7.Controls.Add(this.LineBButton);
            this.flowLayoutPanel7.Controls.Add(this.SoundCaptionLabel);
            this.flowLayoutPanel7.Controls.Add(this.SoundOnButton);
            this.flowLayoutPanel7.Controls.Add(this.AlarmCaptionLabel);
            this.flowLayoutPanel7.Controls.Add(this.AlarmUpDown);
            this.flowLayoutPanel7.Controls.Add(this.AlarmCaption2Label);
            this.flowLayoutPanel7.Location = new System.Drawing.Point(12, 128);
            this.flowLayoutPanel7.Name = "flowLayoutPanel7";
            this.flowLayoutPanel7.Size = new System.Drawing.Size(876, 52);
            this.flowLayoutPanel7.TabIndex = 0;
            //
            // InvokePVButton
            //
            this.InvokePVButton.Location = new System.Drawing.Point(3, 3);
            this.InvokePVButton.Name = "InvokePVButton";
            this.InvokePVButton.Size = new System.Drawing.Size(120, 46);
            this.InvokePVButton.TabIndex = 13;
            this.InvokePVButton.Text = "PV起動";
            this.InvokePVButton.UseVisualStyleBackColor = true;
            this.InvokePVButton.Click += new System.EventHandler(this.InvokePVButton_Click);
            //
            // LineCaptionLabel
            //
            this.LineCaptionLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.LineCaptionLabel.AutoSize = true;
            this.LineCaptionLabel.Location = new System.Drawing.Point(129, 10);
            this.LineCaptionLabel.Name = "LineCaptionLabel";
            this.LineCaptionLabel.Size = new System.Drawing.Size(117, 31);
            this.LineCaptionLabel.TabIndex = 0;
            this.LineCaptionLabel.Text = " 入力端子";
            //
            // LineAButton
            //
            this.LineAButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.LineAButton.Location = new System.Drawing.Point(252, 3);
            this.LineAButton.Name = "LineAButton";
            this.LineAButton.Size = new System.Drawing.Size(46, 46);
            this.LineAButton.TabIndex = 14;
            this.LineAButton.Tag = "require_pv";
            this.LineAButton.Text = "A";
            this.LineAButton.UseVisualStyleBackColor = true;
            this.LineAButton.Click += new System.EventHandler(this.LineAButton_Click);
            //
            // LineBButton
            //
            this.LineBButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.LineBButton.Location = new System.Drawing.Point(304, 3);
            this.LineBButton.Name = "LineBButton";
            this.LineBButton.Size = new System.Drawing.Size(46, 46);
            this.LineBButton.TabIndex = 15;
            this.LineBButton.Tag = "require_pv";
            this.LineBButton.Text = "B";
            this.LineBButton.UseVisualStyleBackColor = true;
            this.LineBButton.Click += new System.EventHandler(this.LineBButton_Click);
            //
            // SoundCaptionLabel
            //
            this.SoundCaptionLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.SoundCaptionLabel.AutoSize = true;
            this.SoundCaptionLabel.Location = new System.Drawing.Point(356, 10);
            this.SoundCaptionLabel.Name = "SoundCaptionLabel";
            this.SoundCaptionLabel.Size = new System.Drawing.Size(69, 31);
            this.SoundCaptionLabel.TabIndex = 0;
            this.SoundCaptionLabel.Text = " 音声";
            //
            // SoundOnButton
            //
            this.SoundOnButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.SoundOnButton.Location = new System.Drawing.Point(431, 3);
            this.SoundOnButton.Name = "SoundOnButton";
            this.SoundOnButton.Size = new System.Drawing.Size(96, 46);
            this.SoundOnButton.TabIndex = 16;
            this.SoundOnButton.Tag = "require_pv";
            this.SoundOnButton.Text = "on/off";
            this.SoundOnButton.UseVisualStyleBackColor = true;
            this.SoundOnButton.Click += new System.EventHandler(this.SoundOnButton_Click);
            //
            // AlarmCaptionLabel
            //
            this.AlarmCaptionLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.AlarmCaptionLabel.AutoSize = true;
            this.AlarmCaptionLabel.Location = new System.Drawing.Point(533, 10);
            this.AlarmCaptionLabel.Name = "AlarmCaptionLabel";
            this.AlarmCaptionLabel.Size = new System.Drawing.Size(117, 31);
            this.AlarmCaptionLabel.TabIndex = 0;
            this.AlarmCaptionLabel.Text = " アラーム";
            //
            // AlarmUpDown
            //
            this.AlarmUpDown.Location = new System.Drawing.Point(656, 3);
            this.AlarmUpDown.Maximum = new decimal(new int[] {1440, 0, 0, 0});
            this.AlarmUpDown.Name = "AlarmUpDown";
            this.AlarmUpDown.Size = new System.Drawing.Size(100, 46);
            this.AlarmUpDown.TabIndex = 17;
            this.AlarmUpDown.Tag = 2;
            this.AlarmUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.AlarmUpDown.Value = new decimal(new int[] {2, 0, 0, 0});
            //
            // AlarmCaption2Label
            //
            this.AlarmCaption2Label.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.AlarmCaption2Label.AutoSize = true;
            this.AlarmCaption2Label.Location = new System.Drawing.Point(762, 10);
            this.AlarmCaption2Label.Name = "AlarmCaption2Label";
            this.AlarmCaption2Label.Size = new System.Drawing.Size(62, 31);
            this.AlarmCaption2Label.TabIndex = 0;
            this.AlarmCaption2Label.Text = "分前";
            //
            // flowLayoutPanel2
            //
            this.flowLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel2.Controls.Add(this.StopTimeCaptionLabel);
            this.flowLayoutPanel2.Controls.Add(this.StopTimeLabel);
            this.flowLayoutPanel2.Controls.Add(this.RemainedTimeCaptionLabel);
            this.flowLayoutPanel2.Controls.Add(this.RemainedTimeLabel);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(892, 70);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(301, 110);
            this.flowLayoutPanel2.TabIndex = 0;
            //
            // MessageTextBox
            //
            this.MessageTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.MessageTextBox.Location = new System.Drawing.Point(12, 187);
            this.MessageTextBox.Multiline = true;
            this.MessageTextBox.Name = "MessageTextBox";
            this.MessageTextBox.ReadOnly = true;
            this.MessageTextBox.Size = new System.Drawing.Size(1182, 108);
            this.MessageTextBox.TabIndex = 0;
            //
            // PvCtrl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1206, 307);
            this.Controls.Add(this.MessageTextBox);
            this.Controls.Add(this.flowLayoutPanel2);
            this.Controls.Add(this.flowLayoutPanel7);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.flowLayoutPanel10);
            this.Font = new System.Drawing.Font("游ゴシック", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "PvCtrl";
            this.Text = "PvCtrl";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PvCtrl_FormClosing);
            this.Load += new System.EventHandler(this.PvCtrl_Load);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.flowLayoutPanel7.ResumeLayout(false);
            this.flowLayoutPanel7.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AlarmUpDown)).EndInit();
            this.flowLayoutPanel10.ResumeLayout(false);
            this.flowLayoutPanel10.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MinUpDown)).EndInit();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label FilenameCaptionLabel;
        private System.Windows.Forms.TextBox FilenameTextBox;
        private System.Windows.Forms.Button FilenamePasteButton;
        private System.Windows.Forms.Label Filler1Label;
        private System.Windows.Forms.Button RecButton;
        private System.Windows.Forms.Button StopButton;
        private System.Windows.Forms.Button InvokePVButton;
        private System.Windows.Forms.Button LineBButton;
        private System.Windows.Forms.Button LineAButton;
        private System.Windows.Forms.Label LineCaptionLabel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel7;
        private System.Windows.Forms.Label SoundCaptionLabel;
        private System.Windows.Forms.Button SoundOnButton;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel10;
        private System.Windows.Forms.Button Min30Button;
        private System.Windows.Forms.Button Min60Button;
        private System.Windows.Forms.Button Min90Button;
        private System.Windows.Forms.Button Min120Button;
        private WheelableNumericUpDown MinUpDown;
        private System.Windows.Forms.Label StopReserveMinLabel;
        private System.Windows.Forms.CheckBox StopReserveCheckBox;
        private System.Windows.Forms.Label StopTimeCaptionLabel;
        private System.Windows.Forms.Label RemainedTimeCaptionLabel;
        private System.Windows.Forms.Label StopTimeLabel;
        private System.Windows.Forms.Label RemainedTimeLabel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Button Min180Button;
        private System.Windows.Forms.Label AlarmCaptionLabel;
        private WheelableNumericUpDown AlarmUpDown;
        private System.Windows.Forms.Label AlarmCaption2Label;
        private System.Windows.Forms.TextBox MessageTextBox;
    }
}

