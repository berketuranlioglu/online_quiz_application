namespace quizserver
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label_port = new System.Windows.Forms.Label();
            this.textbox_port = new System.Windows.Forms.TextBox();
            this.control_panel = new System.Windows.Forms.RichTextBox();
            this.button_listen = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textbox_noquestion = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label_port
            // 
            this.label_port.AutoSize = true;
            this.label_port.Location = new System.Drawing.Point(51, 38);
            this.label_port.Name = "label_port";
            this.label_port.Size = new System.Drawing.Size(32, 15);
            this.label_port.TabIndex = 0;
            this.label_port.Text = "Port:";
            // 
            // textbox_port
            // 
            this.textbox_port.Location = new System.Drawing.Point(89, 35);
            this.textbox_port.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textbox_port.Name = "textbox_port";
            this.textbox_port.Size = new System.Drawing.Size(110, 23);
            this.textbox_port.TabIndex = 1;
            this.textbox_port.Text = "11";
            // 
            // control_panel
            // 
            this.control_panel.Location = new System.Drawing.Point(41, 139);
            this.control_panel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.control_panel.Name = "control_panel";
            this.control_panel.Size = new System.Drawing.Size(418, 156);
            this.control_panel.TabIndex = 2;
            this.control_panel.Text = "";
            // 
            // button_listen
            // 
            this.button_listen.Location = new System.Drawing.Point(191, 80);
            this.button_listen.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_listen.Name = "button_listen";
            this.button_listen.Size = new System.Drawing.Size(82, 22);
            this.button_listen.TabIndex = 3;
            this.button_listen.Text = "Connect";
            this.button_listen.UseVisualStyleBackColor = true;
            this.button_listen.Click += new System.EventHandler(this.button_listen_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(249, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(124, 15);
            this.label1.TabIndex = 4;
            this.label1.Text = "Number of Questions:";
            // 
            // textbox_noquestion
            // 
            this.textbox_noquestion.Location = new System.Drawing.Point(386, 35);
            this.textbox_noquestion.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textbox_noquestion.Name = "textbox_noquestion";
            this.textbox_noquestion.Size = new System.Drawing.Size(41, 23);
            this.textbox_noquestion.TabIndex = 5;
            this.textbox_noquestion.Text = "10";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(499, 338);
            this.Controls.Add(this.textbox_noquestion);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_listen);
            this.Controls.Add(this.control_panel);
            this.Controls.Add(this.textbox_port);
            this.Controls.Add(this.label_port);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label label_port;
        private TextBox textbox_port;
        private RichTextBox control_panel;
        private Button button_listen;
        private Label label1;
        private TextBox textbox_noquestion;
    }
}