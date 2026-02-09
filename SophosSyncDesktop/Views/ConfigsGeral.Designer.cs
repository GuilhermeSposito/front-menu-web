namespace SophosSyncDesktop.Views
{
    partial class ConfigsGeral
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigsGeral));
            panel1 = new Panel();
            BackButton = new PictureBox();
            pictureBox2 = new PictureBox();
            LogoImageSophos = new PictureBox();
            label1 = new Label();
            label2 = new Label();
            tabControleConfigs = new TabControl();
            tabPage1 = new TabPage();
            panel2 = new Panel();
            pictureBox3 = new PictureBox();
            CaminhoParaPastaDeArqNfe = new TextBox();
            label3 = new Label();
            panelCaminhobanco = new Panel();
            pictureBox1 = new PictureBox();
            CaminhoDoSalvamentoDoJson = new TextBox();
            label12 = new Label();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)BackButton).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)LogoImageSophos).BeginInit();
            tabControleConfigs.SuspendLayout();
            tabPage1.SuspendLayout();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            panelCaminhobanco.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel1.BackColor = Color.FromArgb(248, 129, 19);
            panel1.Controls.Add(BackButton);
            panel1.Controls.Add(pictureBox2);
            panel1.Controls.Add(LogoImageSophos);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(label2);
            panel1.Location = new Point(12, 12);
            panel1.Name = "panel1";
            panel1.Size = new Size(947, 74);
            panel1.TabIndex = 1;
            // 
            // BackButton
            // 
            BackButton.BackColor = Color.Transparent;
            BackButton.Cursor = Cursors.Hand;
            BackButton.Image = (Image)resources.GetObject("BackButton.Image");
            BackButton.ImeMode = ImeMode.NoControl;
            BackButton.InitialImage = null;
            BackButton.Location = new Point(874, 2);
            BackButton.Name = "BackButton";
            BackButton.Size = new Size(32, 67);
            BackButton.SizeMode = PictureBoxSizeMode.Zoom;
            BackButton.TabIndex = 5;
            BackButton.TabStop = false;
            BackButton.Click += BackButton_Click;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.Transparent;
            pictureBox2.Cursor = Cursors.Hand;
            pictureBox2.Image = (Image)resources.GetObject("pictureBox2.Image");
            pictureBox2.ImeMode = ImeMode.NoControl;
            pictureBox2.InitialImage = null;
            pictureBox2.Location = new Point(912, 2);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(32, 67);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 4;
            pictureBox2.TabStop = false;
            pictureBox2.Click += pictureBox2_Click;
            // 
            // LogoImageSophos
            // 
            LogoImageSophos.BackColor = Color.Transparent;
            LogoImageSophos.Image = (Image)resources.GetObject("LogoImageSophos.Image");
            LogoImageSophos.ImeMode = ImeMode.NoControl;
            LogoImageSophos.InitialImage = null;
            LogoImageSophos.Location = new Point(-2, 0);
            LogoImageSophos.Name = "LogoImageSophos";
            LogoImageSophos.Size = new Size(262, 73);
            LogoImageSophos.SizeMode = PictureBoxSizeMode.Zoom;
            LogoImageSophos.TabIndex = 2;
            LogoImageSophos.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Microsoft Sans Serif", 22.2F, FontStyle.Bold);
            label1.ImeMode = ImeMode.NoControl;
            label1.Location = new Point(401, 2);
            label1.Name = "label1";
            label1.Size = new Size(240, 42);
            label1.TabIndex = 0;
            label1.Text = "SophosSync";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.ImeMode = ImeMode.NoControl;
            label2.Location = new Point(404, 44);
            label2.Name = "label2";
            label2.Size = new Size(237, 20);
            label2.TabIndex = 1;
            label2.Text = "Aplicativo para impressão da Web";
            // 
            // tabControleConfigs
            // 
            tabControleConfigs.Controls.Add(tabPage1);
            tabControleConfigs.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControleConfigs.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            tabControleConfigs.Location = new Point(12, 91);
            tabControleConfigs.Name = "tabControleConfigs";
            tabControleConfigs.SelectedIndex = 0;
            tabControleConfigs.Size = new Size(947, 464);
            tabControleConfigs.TabIndex = 2;
            tabControleConfigs.DrawItem += tabControleConfigs_DrawItem;
            // 
            // tabPage1
            // 
            tabPage1.BackColor = Color.FromArgb(16, 22, 40);
            tabPage1.BackgroundImage = (Image)resources.GetObject("tabPage1.BackgroundImage");
            tabPage1.BackgroundImageLayout = ImageLayout.Zoom;
            tabPage1.Controls.Add(panel2);
            tabPage1.Controls.Add(panelCaminhobanco);
            tabPage1.Location = new Point(4, 32);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(939, 428);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Config de Salvamento";
            // 
            // panel2
            // 
            panel2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel2.BackColor = Color.FromArgb(25, 36, 54);
            panel2.Controls.Add(pictureBox3);
            panel2.Controls.Add(CaminhoParaPastaDeArqNfe);
            panel2.Controls.Add(label3);
            panel2.Location = new Point(61, 145);
            panel2.Name = "panel2";
            panel2.Size = new Size(820, 112);
            panel2.TabIndex = 6;
            // 
            // pictureBox3
            // 
            pictureBox3.BackColor = Color.Transparent;
            pictureBox3.Cursor = Cursors.Hand;
            pictureBox3.Image = (Image)resources.GetObject("pictureBox3.Image");
            pictureBox3.ImeMode = ImeMode.NoControl;
            pictureBox3.InitialImage = null;
            pictureBox3.Location = new Point(754, 40);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(42, 39);
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox3.TabIndex = 7;
            pictureBox3.TabStop = false;
            pictureBox3.Click += pictureBox3_Click;
            // 
            // CaminhoParaPastaDeArqNfe
            // 
            CaminhoParaPastaDeArqNfe.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            CaminhoParaPastaDeArqNfe.BackColor = Color.FromArgb(16, 22, 40);
            CaminhoParaPastaDeArqNfe.BorderStyle = BorderStyle.None;
            CaminhoParaPastaDeArqNfe.Font = new Font("Segoe UI", 12F);
            CaminhoParaPastaDeArqNfe.ForeColor = Color.White;
            CaminhoParaPastaDeArqNfe.Location = new Point(34, 45);
            CaminhoParaPastaDeArqNfe.Name = "CaminhoParaPastaDeArqNfe";
            CaminhoParaPastaDeArqNfe.Size = new Size(714, 34);
            CaminhoParaPastaDeArqNfe.TabIndex = 3;
            CaminhoParaPastaDeArqNfe.TextChanged += CaminhoParaPastaDeArqNfe_TextChanged;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.Location = new Point(34, 14);
            label3.Name = "label3";
            label3.Size = new Size(366, 28);
            label3.TabIndex = 2;
            label3.Text = "Caminho para o salvamento das NFes";
            // 
            // panelCaminhobanco
            // 
            panelCaminhobanco.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelCaminhobanco.BackColor = Color.FromArgb(25, 36, 54);
            panelCaminhobanco.Controls.Add(pictureBox1);
            panelCaminhobanco.Controls.Add(CaminhoDoSalvamentoDoJson);
            panelCaminhobanco.Controls.Add(label12);
            panelCaminhobanco.Location = new Point(61, 17);
            panelCaminhobanco.Name = "panelCaminhobanco";
            panelCaminhobanco.Size = new Size(820, 112);
            panelCaminhobanco.TabIndex = 5;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.Cursor = Cursors.Hand;
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.ImeMode = ImeMode.NoControl;
            pictureBox1.InitialImage = null;
            pictureBox1.Location = new Point(754, 51);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(42, 39);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 6;
            pictureBox1.TabStop = false;
            pictureBox1.Click += pictureBox1_Click;
            // 
            // CaminhoDoSalvamentoDoJson
            // 
            CaminhoDoSalvamentoDoJson.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            CaminhoDoSalvamentoDoJson.BackColor = Color.FromArgb(16, 22, 40);
            CaminhoDoSalvamentoDoJson.BorderStyle = BorderStyle.None;
            CaminhoDoSalvamentoDoJson.Font = new Font("Segoe UI", 12F);
            CaminhoDoSalvamentoDoJson.ForeColor = Color.White;
            CaminhoDoSalvamentoDoJson.Location = new Point(34, 56);
            CaminhoDoSalvamentoDoJson.Name = "CaminhoDoSalvamentoDoJson";
            CaminhoDoSalvamentoDoJson.Size = new Size(714, 34);
            CaminhoDoSalvamentoDoJson.TabIndex = 3;
            CaminhoDoSalvamentoDoJson.TextChanged += CaminhoDoSalvamentoDoJson_TextChanged;
            // 
            // label12
            // 
            label12.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label12.AutoSize = true;
            label12.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label12.Location = new Point(34, 14);
            label12.Name = "label12";
            label12.Size = new Size(532, 28);
            label12.TabIndex = 2;
            label12.Text = "Caminho para o salvamento do pedido para impressão:";
            // 
            // ConfigsGeral
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(25, 36, 54);
            BackgroundImageLayout = ImageLayout.Center;
            ClientSize = new Size(971, 583);
            Controls.Add(tabControleConfigs);
            Controls.Add(panel1);
            DoubleBuffered = true;
            ForeColor = Color.White;
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "ConfigsGeral";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Configurações";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)BackButton).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)LogoImageSophos).EndInit();
            tabControleConfigs.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            panelCaminhobanco.ResumeLayout(false);
            panelCaminhobanco.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private PictureBox pictureBox2;
        private PictureBox LogoImageSophos;
        private Label label1;
        private Label label2;
        private PictureBox BackButton;
        private TabControl tabControleConfigs;
        private TabPage tabPage1;
        private Panel panelCaminhobanco;
        private TextBox CaminhoDoSalvamentoDoJson;
        private Label label12;
        private Panel panel2;
        private TextBox CaminhoParaPastaDeArqNfe;
        private Label label3;
        private PictureBox pictureBox3;
        private PictureBox pictureBox1;
    }
}