﻿namespace InputPattern
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
            btn_Open = new Button();
            btn_Start = new Button();
            fileList = new ListBox();
            SuspendLayout();
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
            // fileList
            // 
            fileList.FormattingEnabled = true;
            fileList.ItemHeight = 15;
            fileList.Location = new Point(45, 30);
            fileList.Name = "fileList";
            fileList.Size = new Size(300, 349);
            fileList.TabIndex = 3;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(384, 461);
            Controls.Add(fileList);
            Controls.Add(btn_Start);
            Controls.Add(btn_Open);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "3 Oaks Pattern Writer";
            ResumeLayout(false);
        }

        #endregion
        private Button btn_Open;
        private Button btn_Start;
        private ListBox fileList;
    }
}
