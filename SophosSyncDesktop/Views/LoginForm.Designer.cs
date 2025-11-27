namespace SophosSyncDesktop.Views
{
    partial class LoginForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginForm));
            panel1 = new Panel();
            LogoImageSophos = new PictureBox();
            label1 = new Label();
            LblEmail = new Label();
            label2 = new Label();
            textEmail = new TextBox();
            textSenha = new TextBox();
            colorDialog1 = new ColorDialog();
            bTnCancelar = new Button();
            btnEntrar = new Button();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)LogoImageSophos).BeginInit();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(248, 129, 19);
            panel1.Controls.Add(LogoImageSophos);
            panel1.Controls.Add(label1);
            panel1.Location = new Point(12, 12);
            panel1.Name = "panel1";
            panel1.Size = new Size(771, 73);
            panel1.TabIndex = 1;
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
            label1.Location = new Point(426, 15);
            label1.Name = "label1";
            label1.Size = new Size(115, 42);
            label1.TabIndex = 0;
            label1.Text = "Login";
            // 
            // LblEmail
            // 
            LblEmail.AutoSize = true;
            LblEmail.BackColor = Color.Transparent;
            LblEmail.Font = new Font("Microsoft Sans Serif", 14.2F, FontStyle.Bold);
            LblEmail.ImeMode = ImeMode.NoControl;
            LblEmail.Location = new Point(157, 214);
            LblEmail.Name = "LblEmail";
            LblEmail.Size = new Size(86, 29);
            LblEmail.TabIndex = 2;
            LblEmail.Text = "Email:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.Font = new Font("Microsoft Sans Serif", 14.2F, FontStyle.Bold);
            label2.ImeMode = ImeMode.NoControl;
            label2.Location = new Point(157, 315);
            label2.Name = "label2";
            label2.Size = new Size(94, 29);
            label2.TabIndex = 3;
            label2.Text = "Senha:";
            // 
            // textEmail
            // 
            textEmail.Font = new Font("Microsoft Sans Serif", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textEmail.Location = new Point(157, 246);
            textEmail.Name = "textEmail";
            textEmail.Size = new Size(458, 34);
            textEmail.TabIndex = 4;
            textEmail.TextChanged += textEmail_TextChanged;
            // 
            // textSenha
            // 
            textSenha.Font = new Font("Microsoft Sans Serif", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textSenha.Location = new Point(157, 347);
            textSenha.Name = "textSenha";
            textSenha.Size = new Size(458, 34);
            textSenha.TabIndex = 5;
            textSenha.UseSystemPasswordChar = true;
            textSenha.TextChanged += textSenha_TextChanged;
            // 
            // bTnCancelar
            // 
            bTnCancelar.BackColor = Color.Red;
            bTnCancelar.Cursor = Cursors.Hand;
            bTnCancelar.FlatStyle = FlatStyle.Popup;
            bTnCancelar.Font = new Font("Microsoft Sans Serif", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            bTnCancelar.Location = new Point(121, 470);
            bTnCancelar.Name = "bTnCancelar";
            bTnCancelar.Size = new Size(208, 72);
            bTnCancelar.TabIndex = 6;
            bTnCancelar.Text = "Cancelar";
            bTnCancelar.UseVisualStyleBackColor = false;
            bTnCancelar.Click += bTnCancelar_Click;
            // 
            // btnEntrar
            // 
            btnEntrar.BackColor = Color.Lime;
            btnEntrar.Cursor = Cursors.Hand;
            btnEntrar.FlatStyle = FlatStyle.Popup;
            btnEntrar.Font = new Font("Microsoft Sans Serif", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnEntrar.Location = new Point(458, 470);
            btnEntrar.Name = "btnEntrar";
            btnEntrar.Size = new Size(208, 72);
            btnEntrar.TabIndex = 7;
            btnEntrar.Text = "Entrar";
            btnEntrar.UseVisualStyleBackColor = false;
            btnEntrar.Click += btnEntrar_Click;
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(9F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(16, 22, 40);
            ClientSize = new Size(802, 626);
            Controls.Add(btnEntrar);
            Controls.Add(bTnCancelar);
            Controls.Add(textSenha);
            Controls.Add(textEmail);
            Controls.Add(label2);
            Controls.Add(LblEmail);
            Controls.Add(panel1);
            Font = new Font("Microsoft Sans Serif", 9F);
            ForeColor = Color.White;
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "LoginForm";
            Load += LoginForm_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)LogoImageSophos).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panel1;
        private PictureBox LogoImageSophos;
        private Label label1;
        private Label LblEmail;
        private Label label2;
        private TextBox textEmail;
        private TextBox textSenha;
        private ColorDialog colorDialog1;
        private Button bTnCancelar;
        private Button btnEntrar;
    }
}