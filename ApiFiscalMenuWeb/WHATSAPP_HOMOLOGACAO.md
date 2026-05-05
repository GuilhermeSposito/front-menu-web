# WhatsApp Business API — Análise e Requisitos de Homologação

## 1. Análise da Implementação Atual

### 1.1 Estrutura do `SendMessageDtoWS` (comparado com a API oficial)

A estrutura atual está **quase correta**, mas com alguns problemas críticos:

| Campo | Documentação Meta | Implementação atual | Status |
|---|---|---|---|
| `messaging_product` | **Obrigatório**, valor `"whatsapp"` | Presente no DTO ✓ | ✅ OK |
| `to` | E.164 sem `+` (ex: `5511999999999`) | `"55" + telefone` — pode duplicar `55` se o número já vier com DDI | ⚠️ Risco |
| `type` | Obrigatório | Enum serializado como string ✓ | ✅ OK |
| `recipient_type` | Opcional para templates | Retorna `null` para templates (ignorado) | ✅ OK |
| `template.name` | **Obrigatório**, deve ser o nome exato do template aprovado | `TemplatesName.status_pedido` | ✅ Sim, o nome deve ser sempre informado |
| `template.language.code` | **Obrigatório** | `"pt_BR"` ✓ | ✅ OK |
| `template.components` | Opcional, somente se o template tiver variáveis | Presente com header, body e button | ✅ OK |

### 1.2 Sobre o Nome do Template

**Sim, o nome do template deve ser informado em TODA requisição.** O campo `template.name` diz à API qual template pré-aprovado usar. O valor `status_pedido` serializado pelo enum deve bater **exatamente** com o nome cadastrado e aprovado no Meta Business Manager. Se o nome não existir ou estiver com status diferente de `APPROVED`, a mensagem será rejeitada.

### 1.3 Problemas Críticos Encontrados

#### ❌ Problema 1 — Formatação do número de telefone (risco de duplicação de DDI)
```csharp
// MessageService.cs linha 272
To = FormataNumeroWhatsApp(enviaMsgDto.Pedido.Cliente?.Telefone ?? ""),

// FormataNumeroWhatsApp (linha 254)
private static string FormataNumeroWhatsApp(string numero)
{
    var digitos = new string(numero.Where(char.IsDigit).ToArray());
    if (digitos.StartsWith("55")) return digitos;  // ← correto para API oficial
    return $"55{digitos}";
}
```
A função já trata duplicação de DDI. Mas atenção: números sem DDD (7 dígitos) ou números internacionais podem passar inválidos.

#### ❌ Problema 2 — Verify Token hardcoded no Webhook
```csharp
// WhatsAppIntegrationController.cs linha 37
if (mode == "subscribe" && verifyToken == "token")  // "token" hardcoded!
```
O verify token deve vir de configuração (`appsettings.json` / variável de ambiente).

#### ❌ Problema 3 — Assinatura do webhook não validada
O `_webhookSignature` está injetado no controller mas **nunca é usado**. A Meta envia um header `X-Hub-Signature-256` com HMAC-SHA256 do body usando o `App Secret`. Sem validar isso, qualquer um pode enviar requisições falsas para seu webhook.

#### ❌ Problema 4 — Sem resposta automática ao receber mensagem
O método `EndpointDeConexaoComWebHookPost` recebe mensagens mas apenas faz `Console.WriteLine`. Não há envio de resposta ao cliente.

#### ❌ Problema 5 — Sem marcação de mensagem como lida (Mark as Read)
A Meta espera que, ao processar uma mensagem recebida, o sistema marque ela como lida via API, para que o cliente veja os dois checks azuis.

#### ⚠️ Aviso — `HttpSophosClient` sem token na chamada do Gemini (para API oficial)
```csharp
// MessageService.cs linha 329
var HttpSophosClient = _factory.CreateClient("ApiAutorizada");
// Falta: AdicionaTokenNaRequisicao(HttpSophosClient, token);
```
No fluxo `SendMessageStatusOficialAsync`, o token da API Nest não é passado para essa chamada.

---

## 2. O que Implementar para Homologação pela Meta

### 2.1 Requisitos Técnicos (no código)

#### 2.1.1 Validar assinatura do webhook (OBRIGATÓRIO)
A Meta valida que você processa a assinatura antes de aprovar o app.

```csharp
// No POST do webhook, antes de processar:
// 1. Ler o body como raw bytes
// 2. Computar HMAC-SHA256 com o App Secret
// 3. Comparar com o header X-Hub-Signature-256
// 4. Rejeitar com 403 se inválido
```

O `WebhookSignature` já está injetado — basta usá-lo no action POST.

#### 2.1.2 Verify token via configuração
```json
// appsettings.json
{
  "WhatsApp": {
    "VerifyToken": "seu_token_secreto_aqui",
    "AppSecret": "seu_app_secret_aqui",
    "PhoneNumberId": "id_do_numero_meta"
  }
}
```
```csharp
// No controller
var expectedToken = _configuration["WhatsApp:VerifyToken"];
if (mode == "subscribe" && verifyToken == expectedToken) { ... }
```

#### 2.1.3 Resposta automática ao receber mensagem do cliente
Quando o webhook recebe uma mensagem (`mensagem != null`), deve:
1. Marcar como lida (Mark as Read)
2. Enviar mensagem de resposta automática estática

```http
POST /{phone-number-id}/messages
{
  "messaging_product": "whatsapp",
  "status": "read",
  "message_id": "{message-id}"
}
```

```http
POST /{phone-number-id}/messages
{
  "messaging_product": "whatsapp",
  "recipient_type": "individual",
  "to": "{numero-do-cliente}",
  "type": "text",
  "text": {
    "body": "Olá! Recebemos sua mensagem e em breve retornaremos. 😊"
  }
}
```

> **Importante:** Respostas de texto livre (`type: text`) só podem ser enviadas dentro da **janela de 24 horas** após o cliente ter enviado uma mensagem. Fora dessa janela, é obrigatório usar um template aprovado.

#### 2.1.4 Marcar mensagem como lida
```csharp
// Novo método em MessageService ou WhatsAppIntegrationService:
public async Task MarcarMensagemComoLidaAsync(string phoneNumberId, string messageId)
{
    var payload = new {
        messaging_product = "whatsapp",
        status = "read",
        message_id = messageId
    };
    await WSMetaClient.PostAsJsonAsync($"{phoneNumberId}/messages", payload);
}
```

#### 2.1.5 Endpoint de saúde do webhook
A Meta verifica que o endpoint retorna `200 OK` rapidamente. O processamento pesado deve ser feito de forma assíncrona (fila/background task), e o controller deve retornar `200 OK` imediatamente.

```csharp
[HttpPost("endpoint-webhook")]
public async Task<IActionResult> EndpointDeConexaoComWebHookPost([FromBody] WhatsAppWebhookDto dto)
{
    // 1. Validar assinatura (síncrono, rápido)
    // 2. Enfileirar processamento (Background Service / IHostedService)
    return Ok(); // Retornar 200 imediatamente — Meta exige isso
}
```

---

### 2.2 Requisitos de Templates (OBRIGATÓRIO)

Para que os templates sejam aprovados pela Meta:

1. **Criar os templates no Meta Business Manager** em `business.facebook.com` → WhatsApp → Modelos de Mensagem
2. **O template `status_pedido` deve ser criado com exatamente:**
   - Nome: `status_pedido`
   - Idioma: `Português (BR)` — code `pt_BR`
   - Categoria: `UTILITY` (atualizações de pedido/transação) — **não usar `MARKETING`** para notificações de status
   - Header: variável de texto (nome do cliente)
   - Body: variável de texto (mensagem de status)
   - Button URL: variável de sufixo de URL (ID do merchant)
3. **Aguardar aprovação** — pode levar de algumas horas a alguns dias
4. **Templates rejeitados** voltam com motivo; ajustar o conteúdo e resubmeter

---

### 2.3 Requisitos de Negócio / Políticas Meta (OBRIGATÓRIO)

Estes itens são verificados durante a revisão do app:

| Requisito | Descrição |
|---|---|
| **Verificação de Negócio** | A empresa precisa estar verificada no Meta Business Manager (enviar documentos) |
| **Política de Opt-in** | O sistema deve documentar como os clientes optam por receber mensagens. Para pedidos, o opt-in ocorre no momento da compra. |
| **Política de Privacidade** | Deve haver URL de política de privacidade pública acessível |
| **Termos de Serviço** | Aceitar os Termos do WhatsApp Business API |
| **Caso de Uso Declarado** | Declarar na revisão do app que o uso é notificação de status de pedidos (UTILITY) |
| **Número de telefone dedicado** | O número usado deve ser dedicado para o negócio, não pode ser um número pessoal ativo no WhatsApp |
| **Display Name** | O nome exibido no perfil do WhatsApp Business deve ser o nome real do estabelecimento |

---

### 2.4 Fluxo Completo de Resposta Automática (a implementar)

```
Cliente envia mensagem
        │
        ▼
Webhook POST recebe payload
        │
        ├─► Validar assinatura X-Hub-Signature-256
        │         └─► Se inválida: retornar 403
        │
        ├─► Retornar 200 OK imediatamente (Meta exige resposta rápida)
        │
        └─► Background Task:
              │
              ├─► Marcar mensagem como lida (Mark as Read)
              │
              └─► Verificar se está dentro da janela de 24h
                    ├─► Sim: enviar text message (type: text)
                    └─► Não: enviar template aprovado (type: template)
```

---

### 2.5 Configurações Necessárias no `appsettings.json`

```json
{
  "WhatsApp": {
    "VerifyToken": "DEFINIR_TOKEN_SECRETO",
    "AppSecret": "OBTIDO_NO_META_FOR_DEVELOPERS",
    "BaseUrl": "https://graph.facebook.com/v21.0/",
    "AutoReplyMessage": "Olá! Recebemos sua mensagem. Em breve nossa equipe retornará o contato. 😊"
  }
}
```

---

### 2.6 Resumo das Tarefas

- [ ] Mover `VerifyToken` do webhook para `appsettings.json`
- [ ] Implementar validação da assinatura `X-Hub-Signature-256` usando `_webhookSignature`
- [ ] Implementar `MarkAsRead` ao receber mensagem no webhook
- [ ] Implementar envio de resposta automática estática (text message dentro de 24h)
- [ ] Processar o webhook de forma assíncrona (retornar `200 OK` imediatamente)
- [ ] Criar template `status_pedido` no Meta Business Manager com as variáveis corretas
- [ ] Submeter template para aprovação na Meta
- [ ] Verificar o negócio no Meta Business Manager
- [ ] Adicionar URL de política de privacidade no perfil do app
- [ ] Declarar caso de uso `UTILITY` na revisão do app
- [ ] Corrigir chamada do Gemini em `SendMessageStatusOficialAsync` para passar o token

---

*Documento gerado em 05/05/2026 — baseado na análise do código e da documentação oficial da Meta WhatsApp Cloud API.*
