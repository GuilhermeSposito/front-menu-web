# Sophos ERP — Landing Page

Documentação técnica completa do sistema de envio de e-mails via formulário de solicitação de contato/orçamento.

---

## Sumário

1. [Visão Geral da Arquitetura](#1-visão-geral-da-arquitetura)
2. [Estrutura de Arquivos](#2-estrutura-de-arquivos)
3. [Como o Frontend se Conecta ao Backend](#3-como-o-frontend-se-conecta-ao-backend)
4. [Documentação linha a linha — server.ts](#4-documentação-linha-a-linha--serverts)
5. [Documentação linha a linha — App.tsx (formulário)](#5-documentação-linha-a-linha--apptsx-formulário)
6. [Documentação linha a linha — vite.config.ts (proxy)](#6-documentação-linha-a-linha--viteconfigts-proxy)
7. [Variáveis de Ambiente (.env)](#7-variáveis-de-ambiente-env)
8. [Scripts disponíveis](#8-scripts-disponíveis)
9. [Fluxo completo de uma solicitação](#9-fluxo-completo-de-uma-solicitação)
10. [Configuração Gmail (Senha de App)](#10-configuração-gmail-senha-de-app)
11. [Erros comuns e soluções](#11-erros-comuns-e-soluções)

---

## 1. Visão Geral da Arquitetura

O projeto roda **dois servidores simultaneamente**:

```
┌─────────────────────────────────┐     ┌──────────────────────────────────┐
│   Vite Dev Server               │     │   Express Server                 │
│   Porta: 3000 (ou próxima livre)│     │   Porta: 3001                    │
│                                 │     │                                  │
│   Serve o React (frontend)      │────>│   Recebe dados do formulário     │
│   Proxy: /api → localhost:3001  │     │   Envia e-mails via Nodemailer   │
└─────────────────────────────────┘     └──────────────────────────────────┘
```

- O **frontend** (React/Vite) é responsável pela interface visual.
- O **backend** (Express) é responsável por receber os dados do formulário e disparar os e-mails.
- O **proxy** do Vite faz a ponte: quando o frontend faz uma requisição para `/api/...`, o Vite redireciona automaticamente para o servidor Express na porta 3001. O usuário nunca precisa saber que existem duas portas.

---

## 2. Estrutura de Arquivos

```
LandingPage/
├── server.ts          ← Backend Express + Nodemailer (NOVO)
├── vite.config.ts     ← Config do Vite com proxy (MODIFICADO)
├── package.json       ← Dependências e scripts (MODIFICADO)
├── .env               ← Credenciais reais (NÃO versionar no git)
├── .env.example       ← Modelo das variáveis de ambiente (MODIFICADO)
└── src/
    └── App.tsx        ← Formulário com integração à API (MODIFICADO)
```

---

## 3. Como o Frontend se Conecta ao Backend

O caminho de uma solicitação de orçamento percorre 4 etapas:

```
[Usuário preenche o formulário]
         │
         ▼
[App.tsx — handleSubmit()]
  fetch('/api/orcamento', { method: 'POST', body: dados })
         │
         ▼
[Vite proxy intercepta /api/*]
  Redireciona para http://localhost:3001/api/orcamento
         │
         ▼
[server.ts — app.post('/api/orcamento')]
  Valida os campos
  Envia e-mail para a equipe Sophos
  Envia e-mail de confirmação para o cliente
  Retorna { success: true } ou { error: '...' }
         │
         ▼
[App.tsx — trata a resposta]
  Exibe tela de sucesso ou mensagem de erro
```

---

## 4. Documentação linha a linha — `server.ts`

```typescript
import express from 'express';
```
Importa o framework Express, que permite criar um servidor HTTP e definir rotas (endpoints).

```typescript
import nodemailer from 'nodemailer';
```
Importa o Nodemailer, biblioteca responsável por enviar e-mails via protocolo SMTP.

```typescript
import dotenv from 'dotenv';
```
Importa o dotenv, que lê o arquivo `.env` e injeta as variáveis como `process.env.NOME_DA_VARIAVEL`.

```typescript
dotenv.config();
```
Executa a leitura do `.env`. Deve ser chamado antes de qualquer uso de `process.env`, senão as variáveis não estarão disponíveis.

```typescript
const app = express();
```
Cria a instância do servidor Express. Todas as rotas e middlewares são registrados neste objeto `app`.

```typescript
app.use(express.json());
```
Middleware que instrui o Express a interpretar o corpo (body) das requisições como JSON. Sem isso, `req.body` chegaria como `undefined` ao tentar ler os dados do formulário.

```typescript
const PORT = process.env.SERVER_PORT || 3001;
```
Define a porta do servidor. Lê do `.env` (variável `SERVER_PORT`). Se não estiver definida, usa `3001` como padrão.

```typescript
const transporter = nodemailer.createTransport({
  host: process.env.SMTP_HOST,
  port: Number(process.env.SMTP_PORT) || 587,
  secure: process.env.SMTP_SECURE === 'true',
  auth: {
    user: process.env.SMTP_USER,
    pass: process.env.SMTP_PASS,
  },
});
```
Cria o "transportador" de e-mails — a conexão configurada com o servidor SMTP:
- `host`: endereço do servidor de e-mail (ex: `smtp.gmail.com`)
- `port`: porta de comunicação. `587` usa TLS/STARTTLS (mais seguro e recomendado). `465` usa SSL direto.
- `secure`: `false` para porta 587 (negocia TLS depois da conexão), `true` para porta 465 (SSL desde o início)
- `auth.user`: e-mail remetente
- `auth.pass`: senha de app (não a senha normal da conta)

```typescript
app.post('/api/orcamento', async (req, res) => {
```
Registra uma rota HTTP do tipo POST no caminho `/api/orcamento`. Sempre que o frontend fizer `fetch('/api/orcamento', { method: 'POST' })`, este bloco será executado. `async` permite usar `await` dentro da função.

```typescript
  const { empresa, email, whatsapp } = req.body;
```
Extrai os três campos enviados pelo frontend do corpo da requisição. O frontend envia um JSON `{ empresa, email, whatsapp }` e aqui fazemos a desestruturação.

```typescript
  if (!empresa || !email || !whatsapp) {
    res.status(400).json({ error: 'Todos os campos são obrigatórios.' });
    return;
  }
```
Validação básica: se qualquer campo estiver vazio, retorna HTTP 400 (Bad Request) com uma mensagem de erro. O `return` encerra a execução para não continuar para o bloco de envio.

```typescript
  try {
    await transporter.sendMail({
      from: `"Sophos ERP - Solicitação de Contato" <${process.env.SMTP_USER}>`,
      to: process.env.EMAIL_DESTINATARIO,
      subject: `Nova solicitação de contato - ${empresa}`,
      html: `...`,
    });
```
Primeiro envio: e-mail **para a equipe Sophos** notificando sobre o novo lead:
- `from`: remetente com nome amigável + e-mail (formato padrão de e-mail)
- `to`: destinatário interno — definido em `EMAIL_DESTINATARIO` no `.env`
- `subject`: assunto com o nome da empresa do lead
- `html`: corpo do e-mail em HTML com os dados preenchidos no formulário

```typescript
    await transporter.sendMail({
      from: `"Sophos ERP" <${process.env.SMTP_USER}>`,
      to: email,
      subject: 'Recebemos sua solicitação - Sophos ERP',
      html: `...`,
    });
```
Segundo envio: e-mail de **confirmação para o cliente** que preencheu o formulário:
- `to: email` — envia para o e-mail que o próprio cliente digitou no formulário
- Contém um botão com link direto para o WhatsApp da Sophos

```typescript
    res.json({ success: true, message: 'Solicitação enviada com sucesso!' });
```
Se ambos os e-mails forem enviados sem erro, retorna HTTP 200 com `success: true`. O frontend usa esta resposta para exibir a tela de confirmação.

```typescript
  } catch (error) {
    console.error('Erro ao enviar e-mail:', error);
    res.status(500).json({ error: 'Erro ao enviar e-mail. Tente novamente.' });
  }
```
Se qualquer erro ocorrer (credenciais erradas, servidor SMTP fora do ar, etc.), o `catch` captura e retorna HTTP 500 (Internal Server Error) com a mensagem de erro. O detalhe técnico é impresso no terminal para diagnóstico.

```typescript
app.listen(PORT, () => {
  console.log(`Servidor rodando na porta ${PORT}`);
});
```
Inicia o servidor e o coloca para "escutar" requisições na porta definida. A mensagem no console confirma que subiu corretamente.

---

## 5. Documentação linha a linha — `App.tsx` (formulário)

```typescript
type FormStatus = 'idle' | 'loading' | 'success' | 'error';
```
Define um tipo TypeScript com os 4 estados possíveis do formulário:
- `idle`: estado inicial, formulário aguardando preenchimento
- `loading`: requisição em andamento (botão desabilitado)
- `success`: e-mail enviado com sucesso (exibe tela de confirmação)
- `error`: falha no envio (exibe mensagem de erro em vermelho)

```typescript
const [empresa, setEmpresa] = useState('');
const [email, setEmail] = useState('');
const [whatsapp, setWhatsapp] = useState('');
```
Estados controlados para cada campo do formulário. "Controlado" significa que o React gerencia o valor do input — a cada tecla digitada, o estado é atualizado e o input reflete o novo valor. Isso permite ler os valores no momento do submit.

```typescript
const [formStatus, setFormStatus] = useState<FormStatus>('idle');
const [formMessage, setFormMessage] = useState('');
```
- `formStatus`: controla qual tela/estado o formulário exibe
- `formMessage`: armazena a mensagem de retorno da API (sucesso ou erro) para exibir ao usuário

```typescript
async function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
  e.preventDefault();
```
Função chamada quando o formulário é submetido. `e.preventDefault()` cancela o comportamento padrão do navegador (que seria recarregar a página), permitindo que o JavaScript tome controle do envio.

```typescript
  setFormStatus('loading');
  setFormMessage('');
```
Muda o estado para `loading` antes de iniciar a requisição. Isso faz o botão mostrar "Enviando..." e ficar desabilitado, evitando cliques duplos.

```typescript
  const res = await fetch('/api/orcamento', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ empresa, email, whatsapp }),
  });
```
Faz a requisição HTTP para o backend:
- `fetch('/api/orcamento')`: o Vite intercepta este caminho e redireciona para `http://localhost:3001/api/orcamento` via proxy
- `method: 'POST'`: tipo da requisição (envia dados)
- `headers`: informa ao servidor que o corpo está em formato JSON
- `body: JSON.stringify(...)`: converte o objeto JavaScript para texto JSON para ser enviado na requisição

```typescript
  const data = await res.json();
```
Aguarda e converte a resposta do servidor (que também vem em JSON) de volta para um objeto JavaScript.

```typescript
  if (res.ok) {
    setFormStatus('success');
    setFormMessage(data.message || 'Solicitação enviada com sucesso!');
    setEmpresa(''); setEmail(''); setWhatsapp('');
```
`res.ok` é `true` quando o HTTP status é 200–299. Neste caso: muda para tela de sucesso, exibe a mensagem retornada pela API e limpa os campos do formulário.

```typescript
  } else {
    setFormStatus('error');
    setFormMessage(data.error || 'Erro ao enviar. Tente novamente.');
  }
```
Se o status HTTP for 4xx ou 5xx (`res.ok` = false): muda para estado de erro e exibe a mensagem de erro retornada pela API.

```typescript
  } catch {
    setFormStatus('error');
    setFormMessage('Erro de conexão. Tente novamente.');
  }
```
Captura erros de rede (ex: servidor Express não está rodando, sem internet). Diferente dos erros da API acima, este `catch` trata falhas antes mesmo de a requisição chegar ao servidor.

---

## 6. Documentação linha a linha — `vite.config.ts` (proxy)

```typescript
proxy: {
  '/api': 'http://localhost:3001',
},
```
Instrui o servidor de desenvolvimento do Vite a redirecionar todas as requisições cujo caminho começa com `/api` para `http://localhost:3001`.

**Por que isso é necessário?**

O frontend roda em `http://localhost:3000` e o backend em `http://localhost:3001`. Se o frontend tentasse chamar `http://localhost:3001/api/orcamento` diretamente, o navegador bloquearia por **CORS** (política de segurança que impede uma origem de acessar outra origem diferente).

O proxy resolve isso: o frontend faz a requisição para a mesma origem (`/api/orcamento` no mesmo `localhost:3000`), e o Vite — que roda no servidor, não no navegador — faz o redirecionamento internamente, sem acionar o bloqueio de CORS.

```
Sem proxy:   Browser → localhost:3001  ← BLOQUEADO por CORS
Com proxy:   Browser → localhost:3000/api → (Vite redireciona) → localhost:3001  ← OK
```

---

## 7. Variáveis de Ambiente (`.env`)

O arquivo `.env` fica na raiz do projeto e **nunca deve ser commitado no git** (já está no `.gitignore`). Use o `.env.example` como modelo.

| Variável               | Descrição                                            | Exemplo                    |
|------------------------|------------------------------------------------------|----------------------------|
| `SERVER_PORT`          | Porta do servidor Express                            | `3001`                     |
| `SMTP_HOST`            | Endereço do servidor SMTP                            | `smtp.gmail.com`           |
| `SMTP_PORT`            | Porta SMTP                                           | `587`                      |
| `SMTP_SECURE`          | `true` apenas para porta 465 (SSL)                   | `false`                    |
| `SMTP_USER`            | E-mail remetente (quem envia)                        | `sophos@gmail.com`         |
| `SMTP_PASS`            | Senha de App gerada pelo Google (não a senha normal) | `abcdabcdabcdabcd`         |
| `EMAIL_DESTINATARIO`   | E-mail que recebe as solicitações de contato         | `contato@sophosapp.com.br` |
| `GEMINI_API_KEY`       | Chave da API Gemini (uso futuro)                     | —                          |

---

## 8. Scripts disponíveis

```bash
npm run dev          # Inicia apenas o frontend (Vite) na porta 3000
npm run dev:server   # Inicia apenas o backend (Express) na porta 3001
npm run dev:all      # Inicia AMBOS simultaneamente (recomendado para desenvolvimento)
npm run build        # Gera a build de produção do frontend em /dist
npm run lint         # Verifica erros de TypeScript sem compilar
```

> Para que o envio de e-mails funcione, **ambos os servidores precisam estar rodando**. Use sempre `npm run dev:all`.

---

## 9. Fluxo completo de uma solicitação

```
1. Usuário preenche: Empresa, E-mail, WhatsApp
2. Clica em "Enviar Solicitação"
3. handleSubmit() é chamado
4. formStatus → 'loading' (botão mostra "Enviando...")
5. fetch POST /api/orcamento com os dados em JSON
6. Vite proxy redireciona para localhost:3001
7. server.ts valida os campos (se vazio → retorna erro 400)
8. Nodemailer envia E-mail 1 → equipe Sophos (EMAIL_DESTINATARIO)
9. Nodemailer envia E-mail 2 → cliente (e-mail digitado no formulário)
10. server.ts retorna { success: true }
11. handleSubmit() recebe a resposta
12. formStatus → 'success' (exibe tela de confirmação com ícone verde)
13. Campos são limpos automaticamente
```

---

## 10. Configuração Gmail (Senha de App)

O Gmail **não aceita a senha normal** para conexões SMTP externas. É obrigatório usar uma **Senha de App**.

**Passo a passo:**

1. Acesse [myaccount.google.com](https://myaccount.google.com)
2. Vá em **Segurança**
3. Ative a **Verificação em duas etapas** (obrigatório para o próximo passo)
4. Pesquise por **"Senhas de app"** na barra de busca da página
5. Crie uma nova senha com o nome `Sophos ERP`
6. Copie os **16 caracteres gerados** (sem espaços) para `SMTP_PASS` no `.env`

**Outros provedores:**

| Provedor        | SMTP_HOST               | SMTP_PORT | SMTP_SECURE |
|-----------------|-------------------------|-----------|-------------|
| Gmail           | smtp.gmail.com          | 587       | false       |
| Outlook/Hotmail | smtp.office365.com      | 587       | false       |
| Yahoo           | smtp.mail.yahoo.com     | 587       | false       |
| Domínio próprio | mail.seudominio.com.br  | 587       | false       |

---

## 11. Erros comuns e soluções

**`535-5.7.8 Username and Password not accepted`**
- Causa: usando senha normal do Gmail em vez de Senha de App
- Solução: gerar uma Senha de App (ver seção 10)

**`ECONNREFUSED` no frontend (erro de conexão)**
- Causa: servidor Express não está rodando
- Solução: executar `npm run dev:all` em vez de só `npm run dev`

**`Port 3000 is in use`**
- Causa: já existe algo rodando na porta 3000
- Solução: normal — o Vite escolhe automaticamente a próxima porta livre. O proxy continua funcionando

**`Todos os campos são obrigatórios` (erro 400)**
- Causa: formulário enviado com campo vazio
- Solução: todos os inputs têm `required`, mas se a validação falhar no servidor, este erro é retornado

**E-mail chega na caixa de spam**
- Causa: domínio do remetente sem configuração SPF/DKIM
- Solução: para Gmail pessoal isso é normal em testes. Em produção, usar e-mail com domínio próprio configurado corretamente no DNS
