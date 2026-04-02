# Guia de Deploy - Sophos ERP Landing Page

## Arquitetura da Aplicacao

A aplicacao possui dois componentes:

| Componente | Tecnologia | Porta (dev) | Descricao |
|------------|-----------|-------------|-----------|
| Frontend | React + Vite | 3000 | Landing page SPA |
| Backend | Express + Node.js | 3001 | API de envio de emails (`/api/orcamento`) |

---

## Pre-requisitos

- Node.js >= 18
- npm >= 9
- Acesso SMTP configurado (Gmail, Outlook, etc.)

---

## 1. Build do Frontend

```bash
npm install
npm run build
```

Isso gera a pasta `dist/` com os arquivos estaticos otimizados.

---

## 2. Variaveis de Ambiente

Crie um arquivo `.env` no servidor baseado no `.env.example`:

```env
GEMINI_API_KEY="sua_chave_gemini"
APP_URL="https://seudominio.com.br"
SERVER_PORT=3001
SMTP_HOST="smtp.gmail.com"
SMTP_PORT=587
SMTP_SECURE=false
SMTP_USER="seuemail@gmail.com"
SMTP_PASS="sua_senha_de_app"
EMAIL_DESTINATARIO="contato@suaempresa.com.br"
```

> **Gmail:** Use uma [Senha de App](https://support.google.com/accounts/answer/185833), nao a senha da conta.

---

## 3. Opcoes de Deploy

### Opcao A: VPS / Servidor Proprio (Recomendado)

Ideal para manter frontend e backend juntos.

#### 3A.1 Instalar Node.js no servidor

```bash
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt install -y nodejs
```

#### 3A.2 Clonar e buildar

```bash
git clone <url-do-repositorio>
cd FrontMenuWeb/LandingPage
npm install
npm run build
```

#### 3A.3 Servir com PM2 (gerenciador de processos)

```bash
# Instalar PM2 globalmente
npm install -g pm2

# Iniciar o backend
pm2 start server.ts --interpreter ./node_modules/.bin/tsx --name sophos-api

# Verificar status
pm2 status
pm2 logs sophos-api

# Configurar para iniciar com o sistema
pm2 startup
pm2 save
```

#### 3A.4 Configurar Nginx como reverse proxy

Instalar Nginx:

```bash
sudo apt install -y nginx
```

Criar o arquivo de configuracao:

```bash
sudo nano /etc/nginx/sites-available/sophos-landing
```

Conteudo:

```nginx
server {
    listen 80;
    server_name seudominio.com.br www.seudominio.com.br;

    # Frontend - arquivos estaticos
    root /caminho/para/FrontMenuWeb/LandingPage/dist;
    index index.html;

    # Rotas do frontend (SPA)
    location / {
        try_files $uri $uri/ /index.html;
    }

    # Proxy para o backend
    location /api/ {
        proxy_pass http://localhost:3001;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Ativar e reiniciar:

```bash
sudo ln -s /etc/nginx/sites-available/sophos-landing /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

#### 3A.5 HTTPS com Let's Encrypt

```bash
sudo apt install -y certbot python3-certbot-nginx
sudo certbot --nginx -d seudominio.com.br -d www.seudominio.com.br
```

O certificado renova automaticamente.

---

### Opcao B: Docker

#### 3B.1 Criar o Dockerfile

Crie um arquivo `Dockerfile` na raiz do projeto:

```dockerfile
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM node:20-alpine
WORKDIR /app
COPY --from=build /app/dist ./dist
COPY --from=build /app/server.ts ./
COPY --from=build /app/package*.json ./
COPY --from=build /app/node_modules ./node_modules
COPY --from=build /app/.env ./.env
EXPOSE 3001
CMD ["npx", "tsx", "server.ts"]
```

#### 3B.2 Criar o docker-compose.yml

```yaml
version: "3.8"
services:
  sophos-landing:
    build: .
    ports:
      - "3001:3001"
    env_file:
      - .env
    restart: unless-stopped
```

#### 3B.3 Executar

```bash
docker compose up -d --build
```

> Nesta opcao, voce ainda precisa de um Nginx externo (ou Traefik) para servir os arquivos do `dist/` e fazer proxy para a porta 3001.

---

### Opcao C: Vercel (apenas frontend) + Railway/Render (backend)

Se quiser separar frontend e backend em servicos diferentes:

**Frontend na Vercel:**

1. Conecte o repositorio na [Vercel](https://vercel.com)
2. Configure:
   - **Framework Preset:** Vite
   - **Build Command:** `npm run build`
   - **Output Directory:** `dist`
   - **Root Directory:** `FrontMenuWeb/LandingPage`

**Backend no Railway ou Render:**

1. Crie um novo servico apontando para o repositorio
2. Configure o comando de start: `npx tsx server.ts`
3. Adicione as variaveis de ambiente do `.env`
4. Anote a URL gerada (ex: `https://sophos-api.railway.app`)

**Importante:** Nesta opcao, atualize a URL da API no frontend. No `vite.config.ts`, o proxy so funciona em dev. Para producao, ajuste as chamadas `fetch` no `App.tsx` para apontar para a URL do backend em producao, ou configure a variavel `APP_URL`.

---

## 4. Checklist de Deploy

- [ ] Variaveis de ambiente configuradas no servidor
- [ ] `npm run build` executado com sucesso
- [ ] Backend rodando e respondendo em `/api/orcamento`
- [ ] Frontend acessivel no navegador
- [ ] Formulario de contato enviando emails corretamente
- [ ] HTTPS configurado
- [ ] PM2 (ou Docker) configurado para restart automatico
- [ ] DNS apontando para o IP do servidor

---

## 5. Comandos Uteis

```bash
# Verificar se o backend esta rodando
curl -X POST http://localhost:3001/api/orcamento \
  -H "Content-Type: application/json" \
  -d '{"empresa":"Teste","email":"teste@teste.com","whatsapp":"11999999999"}'

# Logs do PM2
pm2 logs sophos-api

# Rebuild apos atualizar codigo
git pull && npm install && npm run build && pm2 restart sophos-api

# Status dos containers Docker
docker compose ps
docker compose logs -f
```

---

## 6. Troubleshooting

| Problema | Solucao |
|----------|---------|
| Emails nao enviam | Verifique SMTP_USER, SMTP_PASS e se a senha de app esta correta |
| 502 Bad Gateway | Backend nao esta rodando. Verifique com `pm2 status` ou `docker compose ps` |
| Pagina em branco | Verifique se `dist/` foi gerado e o `root` do Nginx esta correto |
| CORS errors | Em producao, configure CORS no `server.ts` ou use Nginx como proxy |
| Formulario retorna erro | Verifique os logs do backend: `pm2 logs` ou `docker compose logs` |
