namespace Clent02
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.txtNick = new System.Windows.Forms.TextBox();
            this.btnSign = new System.Windows.Forms.Button();
            this.rtxtMsg = new System.Windows.Forms.RichTextBox();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Nick";
            // 
            // txtNick
            // 
            this.txtNick.Location = new System.Drawing.Point(57, 2);
            this.txtNick.Name = "txtNick";
            this.txtNick.Size = new System.Drawing.Size(159, 21);
            this.txtNick.TabIndex = 1;
            // 
            // btnSign
            // 
            this.btnSign.Location = new System.Drawing.Point(222, 2);
            this.btnSign.Name = "btnSign";
            this.btnSign.Size = new System.Drawing.Size(58, 23);
            this.btnSign.TabIndex = 2;
            this.btnSign.Text = "SignIn";
            this.btnSign.UseVisualStyleBackColor = true;
            this.btnSign.Click += new System.EventHandler(this.button1_Click);
            // 
            // rtxtMsg
            // 
            this.rtxtMsg.Location = new System.Drawing.Point(12, 29);
            this.rtxtMsg.Name = "rtxtMsg";
            this.rtxtMsg.Size = new System.Drawing.Size(268, 203);
            this.rtxtMsg.TabIndex = 3;
            this.rtxtMsg.Text = "";
            // 
            // txtMessage
            // 
            this.txtMessage.Location = new System.Drawing.Point(12, 240);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(204, 21);
            this.txtMessage.TabIndex = 4;
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(222, 240);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(58, 23);
            this.btnSend.TabIndex = 5;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.rtxtMsg);
            this.Controls.Add(this.btnSign);
            this.Controls.Add(this.txtNick);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_Closing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtNick;
        private System.Windows.Forms.Button btnSign;
        private System.Windows.Forms.RichTextBox rtxtMsg;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Button btnSend;
    }
}

