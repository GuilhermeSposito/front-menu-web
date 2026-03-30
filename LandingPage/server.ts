import express from 'express';
import nodemailer from 'nodemailer';
import dotenv from 'dotenv';

dotenv.config();

const app = express();
app.use(express.json());

const PORT = process.env.SERVER_PORT || 3001;

const transporter = nodemailer.createTransport({
  host: process.env.SMTP_HOST,
  port: Number(process.env.SMTP_PORT) || 587,
  secure: process.env.SMTP_SECURE === 'true',
  auth: {
    user: process.env.SMTP_USER,
    pass: process.env.SMTP_PASS,
  },
});

app.post('/api/orcamento', async (req, res) => {
  const { empresa, email, whatsapp } = req.body;

  if (!empresa || !email || !whatsapp) {
    res.status(400).json({ error: 'Todos os campos são obrigatórios.' });
    return;
  }

  try {
    await transporter.sendMail({
      from: `"Sophos ERP - Solicitação de Contato" <${process.env.SMTP_USER}>`,
      to: process.env.EMAIL_DESTINATARIO,
      subject: `Nova solicitação de contato - ${empresa}`,
      html: `
        <div style="font-family: sans-serif; max-width: 600px; margin: 0 auto;">
          <h2 style="color: #F88113;">Nova Solicitação de Contato</h2>
          <p><strong>Empresa:</strong> ${empresa}</p>
          <p><strong>E-mail:</strong> ${email}</p>
          <p><strong>WhatsApp:</strong> ${whatsapp}</p>
        </div>
      `,
    });

    await transporter.sendMail({
      from: `"Sophos ERP" <${process.env.SMTP_USER}>`,
      to: email,
      subject: 'Recebemos sua solicitação - Sophos ERP',
      html: `
        <div style="font-family: sans-serif; max-width: 600px; margin: 0 auto; background: #192436; color: #fff; padding: 40px; border-radius: 16px;">
          <h2 style="color: #F88113;">Olá, ${empresa}!</h2>
          <p>Recebemos sua solicitação de contato e em breve nossa equipe entrará em contato com você.</p>
          <p>Enquanto isso, você também pode nos chamar diretamente pelo WhatsApp:</p>
          <a href="https://wa.me/5516992366175" style="display: inline-block; background: #F88113; color: #fff; padding: 12px 24px; border-radius: 8px; text-decoration: none; font-weight: bold; margin-top: 8px;">
            Falar no WhatsApp
          </a>
          <br><br>
          <p style="color: #94a3b8;">Atenciosamente,<br><strong style="color: #fff;">Equipe Sophos ERP</strong></p>
        </div>
      `,
    });

    res.json({ success: true, message: 'Solicitação enviada com sucesso!' });
  } catch (error) {
    console.error('Erro ao enviar e-mail:', error);
    res.status(500).json({ error: 'Erro ao enviar e-mail. Tente novamente.' });
  }
});

app.listen(PORT, () => {
  console.log(`Servidor rodando na porta ${PORT}`);
});
