namespace SophosSyncDesktop
{
    partial class PaginaInicial
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PaginaInicial));
            SophosSync = new NotifyIcon(components);
            panel1 = new Panel();
            label1 = new Label();
            label2 = new Label();
            LogoImageSophos = new PictureBox();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)LogoImageSophos).BeginInit();
            SuspendLayout();
            // 
            // SophosSync
            // 
            resources.ApplyResources(SophosSync, "SophosSync");
            // 
            // panel1
            // 
            resources.ApplyResources(panel1, "panel1");
            panel1.BackColor = Color.FromArgb(248, 129, 19);
            panel1.Controls.Add(LogoImageSophos);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(label2);
            panel1.Name = "panel1";
            // 
            // label1
            // 
            resources.ApplyResources(label1, "label1");
            label1.BackColor = Color.Transparent;
            label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(label2, "label2");
            label2.Name = "label2";
            // 
            // LogoImageSophos
            // 
            resources.ApplyResources(LogoImageSophos, "LogoImageSophos");
            LogoImageSophos.BackColor = Color.Transparent;
            LogoImageSophos.Name = "LogoImageSophos";
            LogoImageSophos.TabStop = false;
            // 
            // PaginaInicial
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(25, 36, 54);
            Controls.Add(panel1);
            ForeColor = Color.White;
            MaximizeBox = false;
            Name = "PaginaInicial";
            ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;
            FormClosing += PaginaInicial_FormClosing;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)LogoImageSophos).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private NotifyIcon SophosSync;
        private Panel panel1;
        private Label label2;
        private Label label1;
        private PictureBox LogoImageSophos;
    }
}
