namespace InputPattern
{
    partial class MainForm
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
            FileTextBox = new TextBox();
            btn_Open = new Button();
            btn_Start = new Button();
            SuspendLayout();
            // 
            // FileTextBox
            // 
            FileTextBox.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FileTextBox.Location = new Point(40, 30);
            FileTextBox.Multiline = true;
            FileTextBox.Name = "FileTextBox";
            FileTextBox.Size = new Size(300, 350);
            FileTextBox.TabIndex = 0;
            // 
            // btn_Open
            // 
            btn_Open.Location = new Point(100, 400);
            btn_Open.Name = "btn_Open";
            btn_Open.Size = new Size(75, 25);
            btn_Open.TabIndex = 1;
            btn_Open.Text = "Open Data";
            btn_Open.UseVisualStyleBackColor = true;
            btn_Open.Click += btn_Open_Click;
            // 
            // btn_Start
            // 
            btn_Start.Location = new Point(220, 400);
            btn_Start.Name = "btn_Start";
            btn_Start.Size = new Size(75, 25);
            btn_Start.TabIndex = 2;
            btn_Start.Text = "Start";
            btn_Start.UseVisualStyleBackColor = true;
            btn_Start.Click += btn_Start_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(384, 461);
            Controls.Add(btn_Start);
            Controls.Add(btn_Open);
            Controls.Add(FileTextBox);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Hacksaw Pattern Generator";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox FileTextBox;
        private Button btn_Open;
        private Button btn_Start;
    }
}
