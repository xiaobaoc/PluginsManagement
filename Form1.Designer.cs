namespace PluginsManagement
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
            richTextBox1 = new RichTextBox();
            mnsMain = new MenuStrip();
            pluginsMenu = new ToolStripMenuItem();
            mnsMain.SuspendLayout();
            SuspendLayout();
            // 
            // richTextBox1
            // 
            richTextBox1.Location = new Point(27, 107);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(862, 296);
            richTextBox1.TabIndex = 0;
            richTextBox1.Text = "";
            // 
            // mnsMain
            // 
            mnsMain.Items.AddRange(new ToolStripItem[] { pluginsMenu });
            mnsMain.Location = new Point(0, 0);
            mnsMain.Name = "mnsMain";
            mnsMain.Size = new Size(914, 25);
            mnsMain.TabIndex = 1;
            mnsMain.Text = "menuStrip1";
            // 
            // pluginsMenu
            // 
            pluginsMenu.Name = "pluginsMenu";
            pluginsMenu.Size = new Size(44, 21);
            pluginsMenu.Text = "插件";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(914, 503);
            Controls.Add(richTextBox1);
            Controls.Add(mnsMain);
            Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point, 0);
            MainMenuStrip = mnsMain;
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            mnsMain.ResumeLayout(false);
            mnsMain.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private RichTextBox richTextBox1;
        private MenuStrip mnsMain;
        private ToolStripMenuItem pluginsMenu;
    }
}
