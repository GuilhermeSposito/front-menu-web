# Sophos ERP — Front Menu Web

Sistema de gestão para estabelecimentos (restaurantes, lanchonetes, etc.) desenvolvido em **Blazor WebAssembly (.NET 9)**. Integra gestão de pedidos, cardápio digital, caixa, financeiro, emissão fiscal e integrações com iFood e plataformas de entrega.

---

## Sumário

1. [Stack Tecnológico](#stack-tecnológico)
2. [Estrutura do Projeto](#estrutura-do-projeto)
3. [Configuração e Inicialização](#configuração-e-inicialização)
4. [Autenticação e Autorização](#autenticação-e-autorização)
5. [State Management](#state-management)
6. [Services — Detalhamento Completo](#services--detalhamento-completo)
7. [Endpoints da API](#endpoints-da-api)
8. [Páginas e Funcionalidades](#páginas-e-funcionalidades)
9. [Modelos e DTOs](#modelos-e-dtos)
10. [Integrações Externas](#integrações-externas)
11. [Fluxos Principais do Sistema](#fluxos-principais-do-sistema)
12. [Tratamento de Erros](#tratamento-de-erros)
13. [Segurança](#segurança)

---

## Stack Tecnológico

| Tecnologia | Versão | Uso |
|---|---|---|
| Blazor WebAssembly | .NET 9.0 | Framework principal |
| MudBlazor | 8.7.0 | Componentes de UI |
| MudBlazor Extensions | 8.7.0 | Extensões de UI |
| Blazored.LocalStorage | 4.5.0 | Persistência local |
| Microsoft.AspNetCore.Components.Authorization | — | Autenticação/Autorização |
| SignalR Client | 9.0.10 | Comunicação em tempo real |
| Socket.IO Client | 3.1.2 | Websockets para pedidos |
| QRCoder | 1.7.0 | Geração de QR Codes |
| System.Text.Json | — | Serialização JSON |

---

## Estrutura do Projeto

```
FrontMenuWeb/
├── Layout/                         # Layouts e estrutura visual
│   ├── MainLayout.razor            # Layout principal autenticado
│   ├── CardapioDigitalLayout.razor # Layout público do cardápio
│   └── NavMenu.razor               # Menu de navegação lateral
│
├── Pages/                          # Páginas roteáveis (Razor Components)
│   ├── Autenticacao/
│   │   └── Login.razor             # Página de login
│   ├── Cadastros/
│   │   ├── EmpresasIfood.razor     # Cadastro de empresas iFood
│   │   └── EmpresasEntrega.razor   # Cadastro de empresas de entrega
│   ├── CardapioDigital/
│   │   ├── CardapioPublico.razor   # Cardápio público (/cardapio/{merchantId})
│   │   └── QRMesa.razor            # Cardápio por mesa (/qr-mesa/{merchantId}/{tableId})
│   ├── Configuracoes/
│   │   ├── Integracoes.razor       # Configuração de integrações
│   │   ├── Impressoes.razor        # Configuração de impressoras
│   │   └── ConfigGeral.razor       # Configurações gerais do estabelecimento
│   ├── Estastisticas/
│   │   ├── CaixasFechados.razor    # Histórico de caixas fechados
│   │   ├── HistoricoVendas.razor   # Histórico e analytics de vendas
│   │   ├── NFEstatisticas.razor    # Estatísticas de NF-e
│   │   └── VendasPorItem.razor     # Vendas agrupadas por produto
│   ├── Financeiro/
│   │   ├── Categorias.razor        # Categorias financeiras
│   │   ├── Contas.razor            # Contas bancárias
│   │   ├── Formas.razor            # Formas de recebimento
│   │   ├── Lancamentos.razor       # Lançamentos financeiros
│   │   └── Metodos.razor           # Métodos de pagamento do merchant
│   ├── Pessoas/
│   │   └── Pessoas.razor           # Clientes e contatos
│   ├── ProdutosPage/
│   │   ├── Produtos.razor          # Catálogo de produtos
│   │   ├── Grupos.razor            # Grupos de produtos
│   │   ├── Complementos.razor      # Complementos/adicionais
│   │   └── Aliquotas.razor         # Alíquotas fiscais
│   └── Vendas/
│       ├── Home.razor              # Dashboard principal
│       ├── GestaoDePedidos.razor   # Gestão e fulfillment de pedidos
│       ├── FilaDigital.razor       # Fila de balcão
│       ├── Expedidores.razor       # Gestão de expedição
│       ├── Caixa.razor             # Caixa registradora e pagamentos
│       ├── GestaoDeMesas.razor     # Gestão de mesas
│       ├── Funcionarios.razor      # Cadastro de funcionários
│       ├── Motoboys.razor          # Cadastro de motoboys
│       └── Mesas.razor             # Cadastro de mesas/comandas
│
├── Components/                     # Componentes reutilizáveis
│   └── Modais/                     # Modais organizados por domínio
│       ├── Produtos/
│       ├── Pedidos/
│       ├── Mesas/
│       ├── Financeiro/
│       └── Integracoes/
│
├── Services/                       # Lógica de negócio e integração com APIs
│   ├── GrupoServices.cs
│   ├── ProdutoService.cs
│   ├── ComplementosServices.cs
│   ├── PedidosService.cs
│   ├── CaixaEPagamentosService.cs
│   ├── MesasServices.cs
│   ├── MerchantServices.cs
│   ├── PessoasService.cs
│   ├── FuncionariosService.cs
│   ├── MotoboyService.cs
│   ├── AliquotaService.cs
│   ├── EntregasService.cs
│   ├── EntregasMachineService.cs
│   ├── LogoutService.cs
│   ├── BalancaService.cs
│   ├── FinanceroServices/
│   │   ├── CategoriasService.cs
│   │   ├── ContasService.cs
│   │   ├── FormasDeRecebimentoService.cs
│   │   ├── LancamentoFinanceiroService.cs
│   │   └── MetodosDePagMerchantService.cs
│   ├── Fiscal/
│   │   ├── NfService.cs
│   │   └── MessageWhatsAppService.cs
│   └── IntegracoesServices/
│       ├── EmpresaIfoodService.cs
│       ├── IntegracoesSophosService.cs
│       └── ServicosDeTerceiros/
│           ├── CEPService.cs
│           ├── CnpjPesquisaService.cs
│           ├── DistanciasService.cs
│           └── MachineService.cs
│
├── Models/                         # Entidades e modelos do domínio
│   ├── Financeiro/
│   ├── Merchant/
│   ├── Pedidos/
│   ├── Pessoas/
│   ├── Produtos/
│   ├── Integracoes/
│   └── Raios/
│
├── DTOS/                           # Data Transfer Objects
├── AppState.cs                     # Estado global da aplicação
├── PedidoState.cs                  # Estado do carrinho/cardápio digital
├── CustomAuthStateProvider.cs      # Provedor de autenticação customizado
├── CustomAuthorizationMessageHandler.cs  # Interceptor HTTP com HMAC
├── GlobalErrorHandler.cs           # Handler global de erros
├── Program.cs                      # Configuração e injeção de dependências
└── wwwroot/
    ├── appsettings.json            # Configurações da aplicação
    ├── index.html                  # Host HTML com Socket.IO
    └── css/, js/, images/
```

---

## Configuração e Inicialização

### `appsettings.json`

```json
{
  "Api": {
    "BaseUrl": "http://localhost:3030/",
    "BaseUrlAPiFiscal": "http://localhost:5188/",
    "UrlApiIfood": "https://merchant-api.ifood.com.br/",
    "UrlWebSoket": "https://sophos-erp.com.br"
  },
  "ApiKeyNest": "<chave-api-hmac>",
  "HMAC_SECRET": "<segredo-para-assinatura-hmac>"
}
```

### `Program.cs` — Clientes HTTP Registrados

| Nome do Cliente | Base URL | Propósito |
|---|---|---|
| `ApiAutorizada` | `Api:BaseUrl` | API principal — todas as chamadas autenticadas |
| `ApiIntegracoes` | `Api:BaseUrlAPiFiscal` | API fiscal (NF-e, NFC-e, WhatsApp) |
| `ApiRefresh` | `Api:BaseUrl` | Exclusivo para refresh de token (sem interceptor) |
| `CEPService` | `https://viacep.com.br/` | Consulta de CEP |
| `CnpjPesquisaService` | `https://brasilapi.com.br/` | Consulta de CNPJ |
| `MachineService` | `https://sophos-erp.com.br/api-entregas-sophos/v1/` | API de entregas Sophos |

### Serviços Registrados como Singleton/Scoped

```csharp
// Estado global
builder.Services.AddSingleton<AppState>();
builder.Services.AddSingleton<PedidoState>();

// Autenticação
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CustomAuthStateProvider>());

// Todos os services de domínio registrados como Scoped
builder.Services.AddScoped<ProdutoService>();
builder.Services.AddScoped<GrupoServices>();
builder.Services.AddScoped<PedidosService>();
// ... demais services
```

---

## Autenticação e Autorização

### Fluxo de Login

```
Usuário         Login.razor      AuthService        API /auth/login
   |                |                |                    |
   |--- email/senha ->|               |                    |
   |                |-- POST /auth/login ---------------->|
   |                |                |<-- { token, ... } --|
   |                |-- Salva token no LocalStorage        |
   |                |-- SetAuthStateAsync()                |
   |                |-- Redirect para /                    |
```

1. Usuário preenche email e senha na página `/login`
2. `POST /auth/login` com `LoginModel { Email, Senha, IsAdmin }`
3. API retorna JWT token
4. Token armazenado via `Blazored.LocalStorage`
5. `CustomAuthStateProvider.SetAuthStateAsync()` atualiza o estado de autenticação
6. Redirect para dashboard `/`

### Validação de Token na Inicialização

Ao abrir a aplicação, `CustomAuthStateProvider.GetAuthenticationStateAsync()`:

1. Tenta recuperar token do `LocalStorage`
2. Se existe token: chama `GET /merchants/details`
3. Se resposta 200: extrai dados do merchant e monta `ClaimsPrincipal`
4. Se falhar: retorna estado anônimo (não autenticado)

### Claims Disponíveis

| Claim | Valor |
|---|---|
| `ClaimTypes.NameIdentifier` | Nome do merchant |
| `ClaimTypes.Email` | E-mail do merchant |
| `ClaimTypes.Name` | Nome de exibição |
| `merchant_id` | ID do merchant |
| `razao_social` | Razão social |
| `nome_fantasia` | Nome fantasia |
| `imagem_logo` | URL do logotipo |
| `ativo` | Status ativo (`true/false`) |
| `Merchant` | JSON completo do merchant |
| `emitindo_nfe` | Se emite NF-e em produção |

### `CustomAuthorizationMessageHandler` — Interceptor HMAC

Todas as requisições via `ApiAutorizada` passam por este handler:

```
Requisição HTTP saindo
        |
        v
CustomAuthorizationMessageHandler
        |
        |-- Gera x-timestamp (Unix ms)
        |-- Gera x-hash = HMAC-SHA256(timestamp, HMAC_SECRET)
        |-- Adiciona headers:
        |       x-api-key: <ApiKeyNest>
        |       x-timestamp: <timestamp>
        |       x-hash: <hash>
        |-- Credentials: Include (cookies)
        |
        v
    Servidor (valida assinatura)
        |
   [401 Unauthorized?]
        |-- POST /auth/refresh
        |-- Atualiza token
        |-- Reenvia requisição original
        |
   [Refresh falhou?]
        |-- Executa logout
```

### Refresh de Token

- Ao receber `401`, o handler chama `POST /auth/refresh` via `ApiRefresh` (sem interceptor para evitar loop)
- Se sucesso: armazena novo token e reenvia a requisição original
- Se falhar: `LogoutService.Logout()` é chamado

---

## State Management

### `AppState` — Estado Global da Aplicação

```csharp
public class AppState
{
    public ClsMerchant MerchantLogado { get; set; }  // Merchant autenticado
    public bool CaixaAberto { get; set; }            // Status do caixa
}
```

Registrado como `Singleton` — compartilhado por toda a sessão.

### `PedidoState` — Estado do Cardápio Digital

```csharp
public class PedidoState
{
    public ClsMerchant? Merchant { get; set; }
    public List<ClsFormaDeRecebimento>? FormasDeRecebimento { get; set; }
    public ClsPedido? NovoPedido { get; set; }  // Pedido sendo montado pelo cliente
}
```

Usado exclusivamente pelas páginas públicas de cardápio.

### Atualizações em Tempo Real via Socket.IO / SignalR

`PedidosService` expõe delegates estáticos:

| Delegate | Quando é acionado |
|---|---|
| `PedidoRecebido` | Novo pedido (delivery/balcão) chegou |
| `PedidoMesaRecebido` | Novo pedido de mesa chegou |
| `PedidoMudouEtapa` | Status/etapa do pedido alterado |
| `PedidoMudouInfoAdicional` | Informações adicionais do pedido atualizadas |
| `PedidoMesaFechada` | Conta da mesa foi fechada |

O Socket.IO é inicializado em `index.html` conectando ao `UrlWebSoket` configurado.

---

## Services — Detalhamento Completo

### `ProdutoService`

Gerencia o catálogo completo de produtos.

| Método | HTTP | Endpoint | Descrição |
|---|---|---|---|
| `GetAllProdutos()` | GET | `/produtos` | Lista todos os produtos |
| `GetProdutoById(id)` | GET | `/produtos/{id}` | Busca produto por ID |
| `GetProdutoByCodigoInterno(merchantId, code)` | GET | `/produtos/codigo-interno/{merchantId}/{code}` | Busca por código interno |
| `GetProdutosPaginados(page, limit)` | GET | `/produtos/pagination` | Lista paginada |
| `GetProdutosAutoComplete(query)` | GET | `/produtos/find/auto-complete` | Busca para autocomplete |
| `GetProdutosFracionados()` | GET | `/produtos/fracionados` | Produtos vendidos por peso/fração |
| `GetCEST(ncm)` | GET | `/produtos/cest?codigo={ncm}` | Consulta CEST pelo NCM |
| `CreateProduto(produto)` | POST | `/produtos/create` | Cria produto |
| `UpdateProduto(id, produto)` | PATCH | `/produtos/{id}` | Atualiza produto |
| `UpdateImagem(id, imagem)` | PATCH | `/produtos/update/imagem/{id}` | Atualiza imagem |
| `UpdatePreco(id, preco)` | PATCH | `/produtos/preco/modificar/{id}` | Modifica preço |
| `AddPreco(id, preco)` | POST | `/produtos/preco/adicionar/{id}` | Adiciona tamanho/preço |
| `DeletePreco(id)` | DELETE | `/produtos/preco/deletar/{id}` | Remove tamanho/preço |
| `DeleteProduto(id)` | DELETE | `/produtos/{id}` | Remove produto |

---

### `GrupoServices`

Gerencia os grupos/categorias de produtos no cardápio.

| Método | HTTP | Endpoint | Descrição |
|---|---|---|---|
| `GetAllGrupos()` | GET | `/grupos` | Lista todos os grupos |
| `CreateGrupo(grupo)` | POST | `/grupos` | Cria grupo |
| `UpdateGrupo(id, grupo)` | PATCH | `/grupos/{id}` | Atualiza grupo |
| `DeleteGrupo(id)` | DELETE | `/grupos/{id}` | Remove grupo |

---

### `ComplementosServices`

Gerencia complementos e adicionais vinculados aos produtos.

| Método | HTTP | Endpoint | Descrição |
|---|---|---|---|
| `GetGruposDeComplementos()` | GET | `/complementos/grupo-de-complementos` | Lista grupos de complementos |
| `GetGrupoDeComplementoById(id)` | GET | `/complementos/grupo-de-complementos/{id}` | Detalhes de um grupo |
| `GetComplementoById(id)` | GET | `/complementos/{id}` | Detalhes de um complemento |
| `CreateComplemento(complemento)` | POST | `/complementos` | Cria complemento |
| `UpdateComplemento(id, complemento)` | PATCH | `/complementos/{id}` | Atualiza complemento |
| `DeleteComplemento(id)` | DELETE | `/complementos/{id}` | Remove complemento |
| `VincularComplementoAoProduto(dto)` | POST | `/complementos/produtos` | Associa complemento ao produto |
| `DesvincularComplementoDoProduto(id)` | DELETE | `/complementos/produtos/{id}` | Remove associação |

---

### `PedidosService`

Núcleo do sistema de pedidos. Gerencia todo o ciclo de vida de um pedido, desde criação até finalização.

| Método | HTTP | Endpoint | Descrição |
|---|---|---|---|
| `GetPedidos()` | GET | `/pedidos` | Lista pedidos ativos |
| `GetPedidosFinalizados(page, limit)` | GET | `/pedidos/finalizados` | Lista pedidos concluídos (paginado) |
| `GetPedidoById(id)` | GET | `/pedidos/{id}` | Detalhes de um pedido |
| `GetPedidosByMesa(mesaId)` | GET | `/pedidos/mesas/{mesaId}` | Pedidos de uma mesa |
| `GetPedidoByIntegracaoId(id)` | GET | `/pedidos/ped-integracao/{integrationId}` | Pedido por ID de integração |
| `CreatePedido(pedido)` | POST | `/pedidos` | Cria pedido (delivery/balcão) |
| `CreatePedidoMesa(pedido)` | POST | `/pedidos/mesa` | Cria pedido de mesa |
| `UpdatePedido(id, pedido)` | PATCH | `/pedidos/editar/{id}` | Edita pedido |
| `MarcarImpresso(id)` | PUT | `/pedidos/impresso/{id}` | Marca como impresso |
| `MarcarPreparando(id)` | PUT | `/pedidos/preparando/{id}` | Avança para "Em preparo" |
| `MarcarDespachado(id)` | PUT | `/pedidos/despachado/{id}` | Avança para "Despachado" |
| `MarcarFinalizado(id)` | PUT | `/pedidos/finalizado/{id}` | Finaliza pedido |
| `CancelarPedido(id)` | DELETE | `/pedidos/cancelar/{id}` | Cancela pedido |
| `UpdateInfosAdicionais(id, dto)` | PATCH | `/pedidos/infos/{id}` | Atualiza observações/dados |
| `GetEstatisticasItens()` | GET | `/pedidos/estatisticas/itens` | Estatísticas por item |
| `GetEstatisticasGrupos()` | GET | `/pedidos/estatisticas/grupos` | Estatísticas por grupo |
| `GetEstatisticasFormas()` | GET | `/pedidos/estatisticas/formasderecebimento` | Estatísticas por forma de pagamento |

**Etapas (`EtapaPedido`):** `NOVO → PREPARANDO → DESPACHADO → PRONTO → FINALIZADO`

**Status (`StatusPedido`):** `ABERTO → FECHANDO → FECHADO → CANCELADO → FINALIZADO`

**Tipos (`TipoDePedido`):** `BALCAO | MESA | DELIVERY | RETIRADA`

---

### `CaixaEPagamentosService`

Controla o caixa do estabelecimento e os pagamentos vinculados a pedidos.

| Método | HTTP | Endpoint | Descrição |
|---|---|---|---|
| `GetCaixaAberto()` | GET | `/caixas/aberto` | Verifica se caixa está aberto |
| `AbrirCaixa(dto)` | POST | `/caixas` | Abre o caixa com valor inicial |
| `FecharCaixa(dto)` | POST | `/caixas/fechar` | Fecha o caixa com sangria |
| `GetStatusOperacional()` | POST | `/caixas/status` | Status operacional atual |
| `GetCaixasFechados(page)` | GET | `/caixas/fechados` | Histórico de caixas fechados |
| `GetPagamentosByPedido(pedidoId)` | GET | `/caixas/pagamentos/{pedidoId}` | Pagamentos de um pedido |
| `CreatePagamento(pedidoId, pagamento)` | POST | `/caixas/pagamentos/create/{pedidoId}` | Registra pagamento |
| `DeletePagamento(id)` | DELETE | `/caixas/pagamentos/{id}` | Remove pagamento |

---

### `MesasServices`

Gerencia mesas e comandas do estabelecimento.

| Método | HTTP | Endpoint | Descrição |
|---|---|---|---|
| `GetAllMesas()` | GET | `/mesas-comandas` | Lista todas as mesas |
| `GetMesaById(id)` | GET | `/mesas-comandas/{id}` | Detalhes de uma mesa |
| `CreateMesa(mesa)` | POST | `/mesas-comandas` | Cria mesa/comanda |
| `UpdateMesa(id, mesa)` | PATCH | `/mesas-comandas/{id}` | Atualiza mesa |
| `DeleteMesa(id)` | DELETE | `/mesas-comandas/{id}` | Remove mesa |

---

### `MerchantServices`

Gerencia os dados do estabelecimento (merchant) logado.

| Método | HTTP | Endpoint | Descrição |
|---|---|---|---|
| `GetMerchantDetails()` | GET | `/merchants/details` | Dados completos do merchant |
| `UpdateMerchant(merchant)` | PATCH | `/merchants` | Atualiza dados gerais |
| `GetEnderecos()` | GET | `/enderecos-merchant` | Lista endereços |
| `CreateEndereco(endereco)` | POST | `/enderecos-merchant` | Adiciona endereço |
| `UpdateEndereco(id, endereco)` | PATCH | `/enderecos-merchant/{id}` | Atualiza endereço |
| `DeleteEndereco(id)` | DELETE | `/enderecos-merchant/{id}` | Remove endereço |
| `GetDocumentos()` | GET | `/documentos-merchant` | Lista documentos (CNPJ, IE, etc.) |
| `CreateDocumento(doc)` | POST | `/documentos-merchant` | Adiciona documento |
| `UpdateDocumento(id, doc)` | PATCH | `/documentos-merchant/{id}` | Atualiza documento |
| `DeleteDocumento(id)` | DELETE | `/documentos-merchant/{id}` | Remove documento |

---

### `PessoasService`

Gerencia clientes e contatos do estabelecimento.

| Método | HTTP | Endpoint | Descrição |
|---|---|---|---|
| `GetAllPessoas()` | GET | `/pessoas` | Lista todos os clientes |
| `GetPessoaById(id)` | GET | `/pessoas/{id}` | Detalhes de um cliente |
| `CreatePessoa(pessoa)` | POST | `/pessoas` | Cadastra cliente |
| `UpdatePessoa(id, pessoa)` | PATCH | `/pessoas/{id}` | Atualiza cliente |
| `DeletePessoa(id)` | DELETE | `/pessoas/{id}` | Remove cliente |
| `GetEnderecos(pessoaId)` | GET | `/pessoas/endereco/{pessoaId}` | Endereços do cliente |
| `CreateEndereco(endereco)` | POST | `/pessoas/endereco` | Adiciona endereço |
| `UpdateEndereco(id, endereco)` | PATCH | `/pessoas/endereco/{id}` | Atualiza endereço |
| `DeleteEndereco(id)` | DELETE | `/pessoas/endereco/{id}` | Remove endereço |

---

### `FuncionariosService`

Gerencia os funcionários do estabelecimento.

| Método | HTTP | Endpoint | Descrição |
|---|---|---|---|
| `GetAllFuncionarios()` | GET | `/funcionarios` | Lista funcionários |
| `CreateFuncionario(func)` | POST | `/funcionarios` | Cadastra funcionário |
| `UpdateFuncionario(id, func)` | PATCH | `/funcionarios/{id}` | Atualiza funcionário |
| `DeleteFuncionario(id)` | DELETE | `/funcionarios/{id}` | Remove funcionário |

---

### `MotoboyService`

Gerencia motoboys e o controle de entregas atribuídas a eles.

| Método | HTTP | Endpoint | Descrição |
|---|---|---|---|
| `GetAllMotoboys()` | GET | `/motoboys` | Lista motoboys |
| `CreateMotoboy(motoboy)` | POST | `/motoboys` | Cadastra motoboy |
| `UpdateMotoboy(id, motoboy)` | PATCH | `/motoboys/{id}` | Atualiza motoboy |
| `DeleteMotoboy(id)` | DELETE | `/motoboys/{id}` | Remove motoboy |
| `GetEntregas()` | GET | `/motoboys/entregas` | Lista entregas atribuídas |
| `CreateEntrega(entrega)` | POST | `/motoboys/entregas` | Cria registro de entrega |
| `UpdateEntrega(id, entrega)` | PATCH | `/motoboys/entregas/{id}` | Atualiza entrega |
| `DeleteEntrega(id)` | DELETE | `/motoboys/entregas/{id}` | Remove entrega |

---

### `AliquotaService`

Gerencia alíquotas fiscais usadas nos produtos.

| Método | HTTP | Endpoint | Descrição |
|---|---|---|---|
| `GetAllAliquotas()` | GET | `/aliquotas` | Lista alíquotas |
| `CreateAliquota(aliquota)` | POST | `/aliquotas` | Cria alíquota |
| `UpdateAliquota(id, aliquota)` | PATCH | `/aliquotas/{id}` | Atualiza alíquota |
| `DeleteAliquota(id)` | DELETE | `/aliquotas/{id}` | Remove alíquota |

---

### `EntregasService`

Configura raios de entrega (áreas geográficas com preços por distância).

| Método | HTTP | Endpoint | Descrição |
|---|---|---|---|
| `GetRaios()` | GET | `/api-entregas/raio` | Lista raios de entrega |
| `CreateRaio(raio)` | POST | `/api-entregas/raio` | Cria raio |
| `UpdateRaio(id, raio)` | PATCH | `/api-entregas/raio/{id}` | Atualiza raio |
| `DeleteRaio(id)` | DELETE | `/api-entregas/raio/{id}` | Remove raio |

---

### `EntregasMachineService`

Gerencia integração com a plataforma de entregas Sophos Machine.

| Método | HTTP | Endpoint | Descrição |
|---|---|---|---|
| `GetEmpresas()` | GET | `/empresas-machine-integradas` | Lista empresas integradas |
| `CreateEmpresa(empresa)` | POST | `/empresas-machine-integradas` | Integra empresa |
| `UpdateEmpresa(id, empresa)` | PATCH | `/empresas-machine-integradas/{id}` | Atualiza integração |
| `DeleteEmpresa(id)` | DELETE | `/empresas-machine-integradas/{id}` | Remove integração |

---

### `MachineService` (API Externa — Sophos Entrega)

Envia pedidos para a plataforma de entregas externa.

| Método | HTTP | Endpoint | Descrição |
|---|---|---|---|
| `CadastrarPedido(pedido)` | POST | `/pedidos-machine/cadastrar-pedido` | Envia pedido para entrega |

**Base URL:** `https://sophos-erp.com.br/api-entregas-sophos/v1/`

---

### `EmpresaIfoodService`

Gerencia estabelecimentos integrados ao iFood.

| Método | HTTP | Endpoint | Descrição |
|---|---|---|---|
| `GetEmpresas()` | GET | `/empresas-ifood` | Lista empresas iFood integradas |
| `GetEmpresaById(id)` | GET | `/empresas-ifood/{id}` | Detalhes de uma empresa |
| `CreateEmpresa(empresa)` | POST | `/empresas-ifood` | Integra empresa ao iFood |
| `UpdateEmpresa(id, empresa)` | PATCH | `/empresas-ifood/{id}` | Atualiza integração |
| `DeleteEmpresa(id)` | DELETE | `/empresas-ifood/{id}` | Remove integração |

---

### `IntegracoesSophosService`

Gerencia operações de integração com iFood (aceite/cancelamento de pedidos).

| Método | HTTP | Endpoint | Descrição |
|---|---|---|---|
| `GetCancelationReasons()` | GET | `/integracoes/ifood/cancelation-reasons` | Lista motivos de cancelamento |
| `CancelarPedidoIfood(dto)` | POST | `/integracoes/ifood/cancelation` | Cancela pedido no iFood |

---

### Services Financeiros

#### `CategoriasService`

| Método | HTTP | Endpoint |
|---|---|---|
| `GetAll()` | GET | `/financeiro/categorias` |
| `Create(categoria)` | POST | `/financeiro/categorias` |
| `Update(id, categoria)` | PATCH | `/financeiro/categorias/{id}` |
| `Delete(id)` | DELETE | `/financeiro/categorias/{id}` |

#### `ContasService`

| Método | HTTP | Endpoint |
|---|---|---|
| `GetAll()` | GET | `/financeiro/contas` |
| `Create(conta)` | POST | `/financeiro/contas` |
| `Update(id, conta)` | PATCH | `/financeiro/contas/{id}` |
| `Delete(id)` | DELETE | `/financeiro/contas/{id}` |

#### `FormasDeRecebimentoService`

| Método | HTTP | Endpoint |
|---|---|---|
| `GetAll()` | GET | `/financeiro/formas-recebimento` |
| `Create(forma)` | POST | `/financeiro/formas-recebimento` |
| `Update(id, forma)` | PATCH | `/financeiro/formas-recebimento/{id}` |
| `Delete(id)` | DELETE | `/financeiro/formas-recebimento/{id}` |

#### `LancamentoFinanceiroService`

| Método | HTTP | Endpoint |
|---|---|---|
| `GetAll(page, limit)` | GET | `/financeiro/lancamentos` |
| `Create(lancamento)` | POST | `/financeiro/lancamentos` |
| `Update(id, lancamento)` | PATCH | `/financeiro/lancamentos/{id}` |
| `Delete(id)` | DELETE | `/financeiro/lancamentos/{id}` |

#### `MetodosDePagMerchantService`

| Método | HTTP | Endpoint |
|---|---|---|
| `GetAll()` | GET | `/financeiro/metodos-pagamento` |
| `Create(metodo)` | POST | `/financeiro/metodos-pagamento` |
| `Update(id, metodo)` | PATCH | `/financeiro/metodos-pagamento/{id}` |
| `Delete(id)` | DELETE | `/financeiro/metodos-pagamento/{id}` |

---

### `NfService` — Emissão Fiscal

Gerencia emissão de NF-e e NFC-e via API Fiscal.

| Método | HTTP | Endpoint | Descrição |
|---|---|---|---|
| `GetStatusNfce()` | GET | `/nf/status-nfce` | Status do serviço NFC-e na SEFAZ |
| `EnviarNfce(dto)` | POST | `/nf/enviar-nfce` | Emite NFC-e |
| `EnviarNfe(dto)` | POST | `/nf/enviar-nfe` | Emite NF-e |
| `CancelarNfe(dto)` | POST | `/nf/cancelar-nfe` | Cancela NF-e |
| `GetNotasEmitidas(page)` | GET | `/nf/emitidas` | Histórico de notas |

**Base URL:** `http://localhost:5188/` (API Fiscal separada)

---

### Services de Terceiros

#### `CEPService`

```http
GET https://viacep.com.br/ws/{cep}/json/
```

Retorna logradouro, bairro, cidade, estado a partir do CEP. Usado nos formulários de endereço.

#### `CnpjPesquisaService`

```http
GET https://brasilapi.com.br/api/cnpj/v1/{cnpj}
```

Retorna dados da empresa a partir do CNPJ. Usado no cadastro de merchants e pessoas jurídicas.

#### `DistanciasService`

Calcula distâncias para determinar taxa de entrega conforme raios configurados.

#### `BalancaService`

Integração com balança serial via **JS Interop**:

| Método JS | Descrição |
|---|---|
| `balancaSerial.conectar()` | Conecta à porta serial |
| `balancaSerial.lerPeso()` | Lê peso atual |
| `balancaSerial.desconectar()` | Desconecta a balança |
| `balancaSerial.verificarConexao()` | Verifica se está conectada |

Usado para produtos fracionados vendidos por peso.

---

## Endpoints da API

### URLs Base

| API | URL | Uso |
|---|---|---|
| API Principal | `http://localhost:3030/` | Todos os dados do domínio |
| API Fiscal | `http://localhost:5188/` | NF-e, NFC-e, WhatsApp |
| iFood API | `https://merchant-api.ifood.com.br/` | Integração iFood |
| Sophos Entregas | `https://sophos-erp.com.br/api-entregas-sophos/v1/` | Plataforma de entregas |
| ViaCEP | `https://viacep.com.br/ws/` | Consulta CEP |
| BrasilAPI | `https://brasilapi.com.br/api/` | Consulta CNPJ |

### Formato de Resposta da API Principal

**Resposta padrão `ReturnApiRefatored<T>`:**

```json
{
  "status": "success | error",
  "message": ["Mensagem opcional"],
  "data": {
    "message": [],
    "lista": [...],       // Para listagens
    "objeto": {...},      // Para objetos únicos
    "ExtraData": {}       // Dados extras quando necessário
  }
}
```

**Resposta paginada `PaginatedResponse<T>`:**

```json
{
  "data": [...],
  "total": 100,
  "page": 1,
  "lastPage": 5,
  "tiketMedio": 45.50,
  "vendasHoje": 1250.00,
  "totalPago": 980.00,
  "percentualDePedidos": {
    "Mesa": 30,
    "Balcao": 50,
    "Delivery": 20,
    "TotalPedidos": 100
  }
}
```

---

## Páginas e Funcionalidades

### Dashboard — `/`

- Exibe KPIs do dia: total de vendas, ticket médio, pedidos por tipo
- Gráficos de distribuição por forma de pagamento
- Resumo de pedidos em aberto
- Status do caixa

### Gestão de Pedidos — `/vendas/gestao-pedidos`

Painel principal de operação. Exibe pedidos em colunas por etapa:

```
[NOVOS] → [PREPARANDO] → [DESPACHADO] → [PRONTO] → [FINALIZADO]
```

- Recebe atualizações em tempo real via Socket.IO
- Permite avançar etapa do pedido com clique
- Exibe detalhes do pedido: itens, cliente, endereço, pagamento
- Permite cancelar pedido
- Permite imprimir comanda

### Gestão de Mesas — `/vendas/gestao-mesas`

- Visualização visual de mesas abertas/fechadas
- Permite abrir nova conta para uma mesa
- Exibe pedidos ativos por mesa
- Permite fechar conta com seleção de forma de pagamento
- Integração com impressão de comanda

### Caixa — `/vendas/caixa`

- Abre e fecha o caixa com controle de saldo
- Registro de pagamentos por pedido
- Múltiplas formas de pagamento por pedido (troco, divisão)
- Relatório de fechamento de caixa

### Cardápio Digital — `/cardapio/{merchantId}`

Página **pública** (sem autenticação):

```
Cliente acessa URL → Vê produtos agrupados → Seleciona item →
Escolhe tamanho/complementos → Adiciona ao carrinho →
Preenche dados (nome, telefone, endereço) → Confirma pedido →
Pedido aparece no painel da cozinha em tempo real
```

### QR por Mesa — `/qr-mesa/{merchantId}/{tableId}`

Similar ao cardápio, mas já vincula o pedido à mesa identificada na URL.

### Expedidores — `/vendas/expedidores`

Painel para gestão de entregas em andamento:
- Lista pedidos despachados
- Atribui motoboy à entrega
- Registra confirmação de entrega

### Configurações — `/configuracoes/*`

- **Integrações**: Configura iFood, plataformas de entrega, webhooks
- **Impressões**: Define impressoras por tipo de comanda (cozinha, delivery, atendimento)
- **Geral**: Dados do estabelecimento, logo, tempos de preparo/entrega

### Estatísticas — `/estastisticas/*`

- **Caixas Fechados**: Histórico de abertura/fechamento com totais
- **Histórico de Vendas**: Vendas por período com gráficos
- **NF-e**: Notas emitidas, canceladas, status
- **Vendas por Item**: Ranking de produtos mais vendidos

---

## Modelos e DTOs

### `ClsPedido` — Pedido

```csharp
public class ClsPedido
{
    public string Id { get; set; }
    public string DisplayId { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime ModificadoEm { get; set; }
    public string CriadoPor { get; set; }         // "BALCAO" | "IFOOD" | "CARDAPIO"

    // Tipo e status
    public TipoDePedido TipoDePedido { get; set; } // BALCAO | MESA | DELIVERY | RETIRADA
    public EtapaPedido EtapaPedido { get; set; }   // NOVO | PREPARANDO | DESPACHADO | PRONTO | FINALIZADO
    public StatusPedido StatusPedido { get; set; } // ABERTO | FECHANDO | FECHADO | CANCELADO | FINALIZADO

    // Relacionamentos
    public ClsPessoas? Cliente { get; set; }
    public EnderecoPessoa? EnderecoDeEntrega { get; set; }
    public List<ClsItemPedido> Itens { get; set; }
    public List<PagamentoDoPedido> Pagamentos { get; set; }

    // Valores
    public decimal ValorDosItens { get; set; }
    public decimal TaxaEntregaValor { get; set; }
    public decimal DescontoValor { get; set; }
    public decimal AcrescimoValor { get; set; }
    public decimal ServicoValor { get; set; }
    public decimal ValorTotal { get; set; }

    // Integração iFood
    public string? IfoodID { get; set; }

    // Mesa
    public string? MesaId { get; set; }
    public ClsMesa? Mesa { get; set; }

    // Motoboy
    public string? MotoboyId { get; set; }

    // Observações
    public string? Observacao { get; set; }
}
```

### `ClsProduto` — Produto

```csharp
public class ClsProduto
{
    public string Id { get; set; }
    public string Descricao { get; set; }
    public string CodigoInterno { get; set; }
    public string CodBarras { get; set; }

    // Fiscal
    public string NCM { get; set; }
    public string CEST { get; set; }
    public string csosn { get; set; }
    public string CST { get; set; }
    public string AliquotaId { get; set; }
    public string OrigemProduto { get; set; }
    public string TribPisCofins { get; set; }

    // Preços/Tamanhos
    public List<Preco> Precos { get; set; }
    public bool TamanhoUnico { get; set; }

    // Visibilidade
    public bool OcultaTablet { get; set; }
    public bool OcultaDoGestor { get; set; }
    public bool CardapioDoDia { get; set; }
    public bool ItemResgatavel { get; set; }
    public bool Fracionado { get; set; }

    // Grupo e Complementos
    public string GrupoId { get; set; }
    public ClsGrupo Grupo { get; set; }
    public List<GrupoDeComplementosDoProduto> GruposDeComplementosDoProduto { get; set; }

    // Impressão
    public string ImpressoraComanda1 { get; set; }
    public string ImpressoraComanda2 { get; set; }

    // Imagem
    public string ImagemUrl { get; set; }
}
```

### `Preco` — Preço/Tamanho do Produto

```csharp
public class Preco
{
    public string Id { get; set; }
    public string DescricaoDoTamanho { get; set; }
    public decimal Valor { get; set; }
    public decimal CustosDoInsumo { get; set; }
    public decimal CustoReal { get; set; }
    public decimal PrecoSujetido { get; set; }
    public decimal PorcentagemDeLucro { get; set; }
}
```

### `ClsMerchant` — Estabelecimento

```csharp
public class ClsMerchant
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string RazaoSocial { get; set; }
    public string NomeFantasia { get; set; }
    public string ImagemLogo { get; set; }

    // Fiscal
    public string CertificadoBase64 { get; set; }
    public string SenhaCertificado { get; set; }
    public int CrtMerchant { get; set; }
    public bool EmitindoNfeProd { get; set; }
    public string AliquotaIcms { get; set; }

    // Configurações operacionais
    public bool ImprimeComandasDelivery { get; set; }
    public int TempoDeEntregaEMmin { get; set; }
    public int TempoDePreparacaoEmMin { get; set; }

    // Integrações
    public bool IntegraIfood { get; set; }
    public List<ClsEmpresaIfood> EmpresasIfoodIntegradas { get; set; }

    // Relacionamentos
    public ClsFuncionario FuncionarioLogado { get; set; }
    public List<ClsGrupo> Grupos { get; set; }
    public List<ClsFormaDeRecebimento> FormasDeRecebimento { get; set; }
    public List<EnderecoMerchant> EnderecosMerchant { get; set; }
    public List<DocumentoMerchant> Documentos { get; set; }
}
```

### `ClsPessoas` — Cliente/Pessoa

```csharp
public class ClsPessoas
{
    public string Id { get; set; }
    public string Nome { get; set; }
    public string NomeFantasia { get; set; }
    public string RazaoSocial { get; set; }
    public string CPF { get; set; }
    public string CNPJ { get; set; }
    public string Telefone { get; set; }
    public string Email { get; set; }
    public string Contato { get; set; }
    public decimal Desconto { get; set; }
    public string Refere { get; set; }
    public string Setor { get; set; }
    public string Observacao { get; set; }
    public List<EnderecoPessoa> Enderecos { get; set; }
}
```

### `PagamentoDoPedido` — Pagamento

```csharp
public class PagamentoDoPedido
{
    public string Id { get; set; }
    public string FormaDePagamentoId { get; set; }
    public decimal Valor { get; set; }
    public decimal Desconto { get; set; }
    public ClsCaixa Caixa { get; set; }
    public ClsPedido Pedido { get; set; }
    public ClsFormaDeRecebimento FormaDeRecebimento { get; set; }
}
```

### `ClsCaixa` — Caixa

```csharp
public class ClsCaixa
{
    public string Id { get; set; }
    public DateTime AbreuEm { get; set; }
    public DateTime? FechouEm { get; set; }
    public decimal ValorAbertoEm { get; set; }
    public decimal TotalEntradasCaixa { get; set; }
    public decimal TotalSaidasCaixa { get; set; }
}
```

### DTOs Principais

**`LoginModel`**
```csharp
{ Email, Senha, IsAdmin }
```

**`AbreCaixaDto`**
```csharp
{ ValorInicial }
```

**`FechaCaixaDto`**
```csharp
{ ValorFinal, Sangria, Observacao }
```

**`EnNfCeDto`**
```csharp
{ PedidoId, CPFDestinatario }
```

**`UpdatePedidoInfosAdicionaisDto`**
```csharp
{ Observacao, TempoEstimado, MotoboyId }
```

**`CancelationIfoodObjectDto`**
```csharp
{ IfoodOrderId, CancelationCode, Details }
```

---

## Integrações Externas

### iFood

**Fluxo de integração:**

```
iFood (Webhook) → Backend → EmpresaIfoodService → Frontend (Socket.IO)
                                                         |
                                              PedidoRecebido delegate
                                                         |
                                             GestaoDePedidos.razor
                                                         |
                                          Operador aceita/rejeita pedido
                                                         |
                         IntegracoesSophosService.CancelarPedidoIfood()
                                    ou
                         PedidosService.MarcarPreparando()
```

**Configuração:**
- `EmpresaIfoodService`: Vincula estabelecimento ao iFood
- Merchant precisa ter `IntegraIfood = true`
- Credenciais iFood gerenciadas pelo backend

### Sophos Machine (Entrega)

**Fluxo:**

```
Pedido Delivery criado → Sistema calcula raio/distância →
EntregasMachineService verifica empresas integradas →
MachineService.CadastrarPedido() → API Sophos Entregas →
Motoboy recebe entrega no app →
Status atualizado em tempo real
```

### NF-e / NFC-e

**Pré-requisitos:**
- Certificado digital A1 cadastrado no merchant (Base64 + senha)
- `CrtMerchant` configurado (regime tributário)
- Alíquotas configuradas nos produtos

**Fluxo NFC-e:**

```
Pedido finalizado → Operador solicita NFC-e →
NfService.EnviarNfce(dto) → API Fiscal →
API Fiscal processa com certificado do merchant →
SEFAZ autoriza/rejeita →
XML e DANFE retornados →
Impressão do cupom
```

**Fluxo NF-e:**

Similar ao NFC-e, mas para vendas B2B com dados completos do destinatário.

### ViaCEP

Chamada automática ao digitar um CEP válido em formulários de endereço:

```
Usuário digita CEP → CEPService.BuscarCEP(cep) →
GET https://viacep.com.br/ws/{cep}/json/ →
Preenche campos: logradouro, bairro, cidade, estado
```

### BrasilAPI (CNPJ)

Chamada ao digitar CNPJ em cadastros de pessoa jurídica:

```
Usuário digita CNPJ → CnpjPesquisaService.BuscarCNPJ(cnpj) →
GET https://brasilapi.com.br/api/cnpj/v1/{cnpj} →
Preenche: razão social, nome fantasia, endereço
```

---

## Fluxos Principais do Sistema

### 1. Fluxo de Pedido Balcão

```
1. Operador acessa Gestão de Pedidos
2. Clica em "Novo Pedido" → abre modal
3. Busca cliente (autocomplete) ou cadastra novo
4. Busca produtos (autocomplete com preço)
5. Adiciona complementos/observações por item
6. Define forma de pagamento (ou deixa em aberto)
7. POST /pedidos → pedido criado com etapa NOVO
8. Pedido aparece na coluna "NOVOS" em tempo real
9. Cozinha visualiza e avança para PREPARANDO
10. Operador avança para PRONTO
11. Pagamento registrado em /caixas/pagamentos/create/{pedidoId}
12. Pedido finalizado → vai para histórico
```

### 2. Fluxo de Pedido Delivery

```
1. Cliente acessa /cardapio/{merchantId} ou operador cria no sistema
2. Seleciona produtos, tamanhos e complementos
3. Informa endereço → sistema calcula taxa de entrega por raio
4. Define forma de pagamento
5. POST /pedidos → tipo DELIVERY
6. [Opcional] Sistema envia para Sophos Machine/iFood
7. Cozinha prepara → Motoboy atribuído
8. Etapa avança: PREPARANDO → DESPACHADO
9. Entrega confirmada → FINALIZADO
```

### 3. Fluxo de Mesa

```
1. Operador acessa Gestão de Mesas
2. Seleciona mesa disponível
3. POST /pedidos/mesa → abre comanda da mesa
4. Ao longo do tempo: múltiplos itens adicionados à comanda
5. Cliente pede a conta → operador acessa fechamento
6. Sistema soma todos os itens + serviço (%)
7. Pagamento: único ou dividido entre pessoas
8. POST /caixas/pagamentos/create/{pedidoId} por forma de pagamento
9. Mesa liberada → disponível para próximo cliente
```

### 4. Fluxo do Cardápio Digital (Público)

```
1. Estabelecimento gera QR Code da mesa ou link geral
2. Cliente escaneia → acessa /qr-mesa/{merchantId}/{tableId}
3. Sistema carrega merchant e cardápio (endpoints públicos)
4. Cliente navega grupos → seleciona produto → escolhe tamanho
5. Modal de complementos abre (se configurado)
6. Item adicionado ao PedidoState (cliente local)
7. Cliente revisa carrinho → informa nome/telefone
8. POST /pedidos ou /pedidos/mesa → pedido criado
9. Socket.IO notifica painel da cozinha
10. PedidoRecebido delegate acionado → UI atualiza em tempo real
```

### 5. Fluxo de Abertura e Fechamento de Caixa

```
Abertura:
1. Operador acessa Caixa → "Abrir Caixa"
2. Informa valor inicial (dinheiro em caixa)
3. POST /caixas → CaixaAberto = true em AppState
4. Sistema libera operações de pagamento

Fechamento:
1. Operador clica "Fechar Caixa"
2. Sistema exibe totais: entradas, saídas, saldo esperado
3. Operador informa valor físico contado
4. Sistema calcula diferença (sobra/falta)
5. POST /caixas/fechar → caixa registrado
6. CaixaAberto = false em AppState
7. Relatório disponível em /estastisticas/caixas
```

### 6. Fluxo de Emissão de NFC-e

```
1. Pedido finalizado com pagamento registrado
2. Operador clica "Emitir NFC-e"
3. [Opcional] Informa CPF do consumidor
4. NfService.EnviarNfce() → POST /nf/enviar-nfce
5. API Fiscal valida dados + certificado do merchant
6. Transmite para SEFAZ
7. SEFAZ retorna autorização + XML
8. Sistema exibe DANFE para impressão
9. [Opcional] Envio via WhatsApp (MessageWhatsAppService)
```

---

## Tratamento de Erros

### `GlobalErrorHandler`

Implementa `IErrorBoundaryLogger` — captura exceções não tratadas no Blazor:

```csharp
public class GlobalErrorHandler : IErrorBoundaryLogger
{
    public ValueTask LogErrorAsync(Exception exception)
    {
        // Exibe Snackbar com mensagem de erro
        // Duração: 6 segundos
        // Visível para o usuário
    }
}
```

### Tratamento em Serviços HTTP

```
Requisição HTTP
    |
    v
try { ... }
catch (HttpRequestException)  → "Erro de conexão com o servidor"
catch (TaskCanceledException) → "Tempo de resposta excedido"
catch (Exception)             → Mensagem genérica
    |
    v
ReturnApiRefatored com status "error" e message[]
```

### Erros da API

A API retorna erros no formato:
```json
{
  "status": "error",
  "message": ["Descrição do erro específico"],
  "data": null
}
```

O frontend exibe via `MudSnackbar` os itens do array `message`.

---

## Segurança

### Autenticação HMAC

Cada requisição à API principal inclui:

```
x-api-key:   <ApiKeyNest configurado>
x-timestamp: <Unix timestamp em milissegundos>
x-hash:      HMAC-SHA256(timestamp, HMAC_SECRET)
```

O servidor valida:
1. Se `x-api-key` é válida
2. Se `x-timestamp` está dentro da janela de tempo aceitável (evita replay attacks)
3. Se `x-hash` corresponde ao HMAC calculado com o segredo compartilhado

### JWT Token

- Armazenado em `LocalStorage` via `Blazored.LocalStorage`
- Renovação automática via `POST /auth/refresh` ao receber `401`
- Logout automático se refresh falhar

### Certificados Fiscais

- Certificado A1 armazenado como **Base64** no banco de dados do merchant
- Senha do certificado armazenada junto (necessário para assinar as NF-e)
- Nunca trafegam no frontend além do cadastro inicial

### Controle de Acesso

- Páginas protegidas com `[Authorize]` (Blazor Authorization)
- `CustomAuthStateProvider` valida token antes de renderizar páginas protegidas
- Cardápio digital (páginas `/cardapio/*` e `/qr-mesa/*`) são públicas — não requerem autenticação

---

## Variáveis de Ambiente / Configurações Necessárias

| Chave | Descrição | Exemplo |
|---|---|---|
| `Api:BaseUrl` | URL da API principal | `http://localhost:3030/` |
| `Api:BaseUrlAPiFiscal` | URL da API fiscal | `http://localhost:5188/` |
| `Api:UrlApiIfood` | URL base da API iFood | `https://merchant-api.ifood.com.br/` |
| `Api:UrlWebSoket` | URL do servidor WebSocket | `https://sophos-erp.com.br` |
| `ApiKeyNest` | Chave de API para autenticação HMAC | `<hash>` |
| `HMAC_SECRET` | Segredo para assinatura HMAC-SHA256 | `<segredo>` |

---

*Documentação gerada em 2026-04-10 — Sophos ERP Front Menu Web*
