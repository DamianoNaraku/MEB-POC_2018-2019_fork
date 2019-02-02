namespace broadcastListener
{
    partial class GUI
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelReceived = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.textboxStatus = new System.Windows.Forms.RichTextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.ButtonStat = new System.Windows.Forms.Button();
            this.richTextBoxErrors = new System.Windows.Forms.RichTextBox();
            this.panelMidd = new System.Windows.Forms.Panel();
            this.panelQueue = new System.Windows.Forms.Panel();
            this.panelLabel = new System.Windows.Forms.Panel();
            this.labelQueue = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panelBot = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.richTextBoxmsg = new System.Windows.Forms.RichTextBox();
            this.panel6 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panelreceivedSlave = new System.Windows.Forms.Panel();
            this.richTextBoxSlave = new System.Windows.Forms.RichTextBox();
            this.panel8 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panelMidd.SuspendLayout();
            this.panelQueue.SuspendLayout();
            this.panelLabel.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panelBot.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panelreceivedSlave.SuspendLayout();
            this.panel8.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelReceived
            // 
            this.labelReceived.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelReceived.Location = new System.Drawing.Point(0, 0);
            this.labelReceived.Name = "labelReceived";
            this.labelReceived.Size = new System.Drawing.Size(1064, 13);
            this.labelReceived.TabIndex = 1;
            this.labelReceived.Text = "Errors:";
            this.labelReceived.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.labelReceived);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1064, 16);
            this.panel1.TabIndex = 2;
            // 
            // textboxStatus
            // 
            this.textboxStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textboxStatus.Location = new System.Drawing.Point(0, 0);
            this.textboxStatus.Name = "textboxStatus";
            this.textboxStatus.Size = new System.Drawing.Size(1064, 130);
            this.textboxStatus.TabIndex = 3;
            this.textboxStatus.Text = "";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.ButtonStat);
            this.panel2.Controls.Add(this.richTextBoxErrors);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1064, 100);
            this.panel2.TabIndex = 5;
            // 
            // ButtonStat
            // 
            this.ButtonStat.Location = new System.Drawing.Point(989, 0);
            this.ButtonStat.Margin = new System.Windows.Forms.Padding(0);
            this.ButtonStat.Name = "ButtonStat";
            this.ButtonStat.Size = new System.Drawing.Size(75, 24);
            this.ButtonStat.TabIndex = 2;
            this.ButtonStat.Text = "Statistics";
            this.ButtonStat.UseVisualStyleBackColor = true;
            this.ButtonStat.Click += new System.EventHandler(this.ButtonStat_Click);
            // 
            // richTextBoxErrors
            // 
            this.richTextBoxErrors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxErrors.Location = new System.Drawing.Point(0, 0);
            this.richTextBoxErrors.Name = "richTextBoxErrors";
            this.richTextBoxErrors.Size = new System.Drawing.Size(1064, 100);
            this.richTextBoxErrors.TabIndex = 0;
            this.richTextBoxErrors.Text = "";
            // 
            // panelMidd
            // 
            this.panelMidd.Controls.Add(this.panelQueue);
            this.panelMidd.Controls.Add(this.panelLabel);
            this.panelMidd.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelMidd.Location = new System.Drawing.Point(0, 116);
            this.panelMidd.Name = "panelMidd";
            this.panelMidd.Size = new System.Drawing.Size(1064, 143);
            this.panelMidd.TabIndex = 6;
            // 
            // panelQueue
            // 
            this.panelQueue.Controls.Add(this.textboxStatus);
            this.panelQueue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelQueue.Location = new System.Drawing.Point(0, 13);
            this.panelQueue.Name = "panelQueue";
            this.panelQueue.Size = new System.Drawing.Size(1064, 130);
            this.panelQueue.TabIndex = 8;
            // 
            // panelLabel
            // 
            this.panelLabel.Controls.Add(this.labelQueue);
            this.panelLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelLabel.Location = new System.Drawing.Point(0, 0);
            this.panelLabel.Name = "panelLabel";
            this.panelLabel.Size = new System.Drawing.Size(1064, 13);
            this.panelLabel.TabIndex = 7;
            // 
            // labelQueue
            // 
            this.labelQueue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelQueue.Location = new System.Drawing.Point(0, 0);
            this.labelQueue.Name = "labelQueue";
            this.labelQueue.Size = new System.Drawing.Size(1064, 13);
            this.labelQueue.TabIndex = 4;
            this.labelQueue.Text = "Status Update";
            this.labelQueue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.panel2);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(0, 16);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(1064, 100);
            this.panel4.TabIndex = 6;
            // 
            // panelBot
            // 
            this.panelBot.Controls.Add(this.panel5);
            this.panelBot.Controls.Add(this.panel6);
            this.panelBot.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelBot.Location = new System.Drawing.Point(0, 259);
            this.panelBot.Name = "panelBot";
            this.panelBot.Size = new System.Drawing.Size(1064, 83);
            this.panelBot.TabIndex = 9;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.richTextBoxmsg);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel5.Location = new System.Drawing.Point(0, 13);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(1064, 70);
            this.panel5.TabIndex = 8;
            // 
            // richTextBoxmsg
            // 
            this.richTextBoxmsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxmsg.Location = new System.Drawing.Point(0, 0);
            this.richTextBoxmsg.Name = "richTextBoxmsg";
            this.richTextBoxmsg.Size = new System.Drawing.Size(1064, 70);
            this.richTextBoxmsg.TabIndex = 3;
            this.richTextBoxmsg.Text = "";
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.label1);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel6.Location = new System.Drawing.Point(0, 0);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(1064, 13);
            this.panel6.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(1064, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Received  - Tool";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.panelreceivedSlave);
            this.panel3.Controls.Add(this.panel8);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 342);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1064, 207);
            this.panel3.TabIndex = 10;
            // 
            // panelreceivedSlave
            // 
            this.panelreceivedSlave.Controls.Add(this.richTextBoxSlave);
            this.panelreceivedSlave.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelreceivedSlave.Location = new System.Drawing.Point(0, 13);
            this.panelreceivedSlave.Name = "panelreceivedSlave";
            this.panelreceivedSlave.Size = new System.Drawing.Size(1064, 194);
            this.panelreceivedSlave.TabIndex = 8;
            // 
            // richTextBoxSlave
            // 
            this.richTextBoxSlave.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxSlave.Location = new System.Drawing.Point(0, 0);
            this.richTextBoxSlave.Name = "richTextBoxSlave";
            this.richTextBoxSlave.Size = new System.Drawing.Size(1064, 194);
            this.richTextBoxSlave.TabIndex = 3;
            this.richTextBoxSlave.Text = "";
            // 
            // panel8
            // 
            this.panel8.Controls.Add(this.label2);
            this.panel8.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel8.Location = new System.Drawing.Point(0, 0);
            this.panel8.Name = "panel8";
            this.panel8.Size = new System.Drawing.Size(1064, 13);
            this.panel8.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(1064, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Received - Intra communication";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // GUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(1064, 549);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panelBot);
            this.Controls.Add(this.panelMidd);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel1);
            this.Name = "GUI";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "Broadcast Listener";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GUI_FormClosing);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panelMidd.ResumeLayout(false);
            this.panelQueue.ResumeLayout(false);
            this.panelLabel.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panelBot.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panelreceivedSlave.ResumeLayout(false);
            this.panel8.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelReceived;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panelMidd;
        private System.Windows.Forms.Panel panelLabel;
        private System.Windows.Forms.Label labelQueue;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panelQueue;
        public System.Windows.Forms.RichTextBox textboxStatus;
        public System.Windows.Forms.RichTextBox richTextBoxErrors;
        private System.Windows.Forms.Panel panelBot;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel5;
        public System.Windows.Forms.RichTextBox richTextBoxmsg;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panelreceivedSlave;
        public System.Windows.Forms.RichTextBox richTextBoxSlave;
        private System.Windows.Forms.Panel panel8;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button ButtonStat;
    }
}

