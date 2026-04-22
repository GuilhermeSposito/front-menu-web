# Roteirização de Entregas — Guia de Implementação

> **Instruções para implementar esta feature.** Cada passo tem o arquivo exato a criar/editar, o padrão a seguir (baseado no que já existe no projeto), e um critério de "feito". Seguir na ordem.

---

## Contexto do projeto (pra não esquecer)

- **Backend**: NestJS + TypeORM + PostgreSQL em `c:\Prog\projeto_menu_web\api-proj-menu-web`
- **Frontend**: Blazor WebAssembly + MudBlazor v8 em `c:\Prog\projeto_menu_web\front-menu-web\FrontMenuWeb\FrontMenuWeb`
- **Shared models**: `c:\Prog\projeto_menu_web\front-menu-web\FrontMenuWeb\FrontMenuSharedModels`
- **Padrão de retorno da API**: `ReturnApiRefatored<T>` com `Status`, `Messages`, `Data`. Usa `Data.Lista` (array) ou `Data.Objeto` (objeto único).
- **Auth**: `@UseGuards(AuthGuard)` em todos os controllers. Pega merchant via `@User() merchant: MerchantObj`.
- **Front chama API via**: `HttpClient` injetado, padrão `JsonSerializer.Deserialize<ReturnApiRefatored<T>>(json)`. Ver `CaixaEPagamentosService.cs` como referência.
- **Modais**: MudDialog com `[CascadingParameter] IMudDialogInstance MudDialog`, `DialogParameters<T>`.
- **AppState, DialogService, Snackbar, AuthorizationService**: já injetados globalmente via `_Imports.razor`.

### Funcionalidades do que estamos construindo

Montar uma rota de entregas de pedidos DELIVERY do caixa aberto, arrastando pra um mapa, com opção de otimizar ordem (Google Routes API), imprimir e atribuir todos a um motoboy de uma vez.

### Endpoints/recursos existentes que usamos (não criar de novo)
- `GET /caixas/aberto` → verifica caixa aberto.
- `GET /motoboys?ativo=true` → lista motoboys ativos.
- `POST /motoboys/entregas` → atribui um pedido a um motoboy. Body: `{ motoboyId, pedidoCaixaId, ValorPedido, ValorEntrega }`.
- Entidade `PedidoCaixa` com relação ao `Cliente` e endereço.

---

## PASSO 1 — Backend: Módulo de Roteirização

### 1.1. Criar módulo NestJS

**Arquivo:** `api-proj-menu-web/src/roteirizacao/roteirizacao.module.ts`

Seguir o padrão de `caixas.module.ts`. Importa `TypeOrmModule.forFeature([...])` com as entidades de `PedidoCaixa`, `Cliente`, `Merchant`. Registra controller e service. Adicionar o módulo em `app.module.ts` nos `imports`.

### 1.2. Service

**Arquivo:** `api-proj-menu-web/src/roteirizacao/roteirizacao.service.ts`

Métodos:

```ts
async GetPedidosDeliveryDoCaixaAtual(merchant: MerchantObj, funcionarioId?: number)
  // 1. Busca caixa aberto do funcionário (reusar lógica de CaixasService.RetornaCaixaAberto)
  // 2. Busca pedidos do caixa onde TipoPedido = DELIVERY e Status != CANCELADO
  // 3. Para cada pedido, monta endereço completo do cliente
  // 4. Chama helper Geocode(endereco) — usa cache em memória ou coluna lat/lng no cliente
  // 5. Retorna lista com { pedidoId, cliente, endereco, telefone, valor, lat, lng, erroGeocode }
  // 6. Wrap em ReturnApiRefatored.Success({ Pedidos: [...] })

async OtimizarRota(merchant: MerchantObj, dto: OtimizarRotaDto)
  // 1. Recebe { origemLat, origemLng, pontos: [{ pedidoId, lat, lng }] }
  // 2. Chama Google Routes API com optimizeWaypoints: true
  // 3. Retorna { ordemOtimizada: [pedidoId1, pedidoId2, ...], distanciaKm, tempoMin }
  // 4. Wrap em ReturnApiRefatored.Success({ Rota: {...} })

private async Geocode(endereco: string): Promise<{ lat, lng } | null>
  // Proxy pro Google Geocoding API. Lê GOOGLE_MAPS_API_KEY do .env.
  // Cache simples em Map<endereco, {lat,lng}> em memória (invalidar após X horas).
```

**Config da key**: adicionar `GOOGLE_MAPS_API_KEY` em `.env`. Injetar via `ConfigService`.

### 1.3. DTOs

**Arquivo:** `api-proj-menu-web/src/roteirizacao/Dto's/otimizar-rota.dto.ts`

```ts
class OtimizarRotaDto {
  @IsNumber() origemLat: number;
  @IsNumber() origemLng: number;
  @IsArray() pontos: PontoDto[];
}
class PontoDto {
  @IsNumber() pedidoId: number;
  @IsNumber() lat: number;
  @IsNumber() lng: number;
}
```

### 1.4. Controller

**Arquivo:** `api-proj-menu-web/src/roteirizacao/roteirizacao.controller.ts`

```ts
@UseGuards(AuthGuard)
@Controller('roteirizacao')
export class RoteirizacaoController {
  constructor(private readonly service: RoteirizacaoService) {}

  @Get('pedidos-delivery-do-caixa')
  async GetPedidos(@User() merchant: MerchantObj, @Query('funcionario_id') funcionarioId?: number) {
    return this.service.GetPedidosDeliveryDoCaixaAtual(merchant, funcionarioId);
  }

  @Post('otimizar')
  async Otimizar(@User() merchant: MerchantObj, @Body() dto: OtimizarRotaDto) {
    return this.service.OtimizarRota(merchant, dto);
  }
}
```

### 1.5. Opcional — coluna de cache no Cliente

Se a performance de geocoding for problema, criar migration adicionando `latitudeCache`, `longitudeCache`, `geocodeAtualizadoEm` no `Cliente`. Por enquanto, cache em memória resolve.

**✔ Feito quando:** `curl GET /roteirizacao/pedidos-delivery-do-caixa` (com token) retorna lista com lat/lng preenchidos.

---

## PASSO 2 — Shared Models

**Arquivo:** `FrontMenuSharedModels/Models/Roteirizacao/PedidoParaRota.cs`

```csharp
using System.Text.Json.Serialization;
namespace FrontMenuWeb.Models.Roteirizacao;

public class PedidoParaRota
{
    [JsonPropertyName("pedidoId")] public int PedidoId { get; set; }
    [JsonPropertyName("cliente")] public string Cliente { get; set; } = "";
    [JsonPropertyName("endereco")] public string Endereco { get; set; } = "";
    [JsonPropertyName("telefone")] public string? Telefone { get; set; }
    [JsonPropertyName("valor")] public decimal Valor { get; set; }
    [JsonPropertyName("taxaEntrega")] public decimal TaxaEntrega { get; set; }
    [JsonPropertyName("lat")] public double? Lat { get; set; }
    [JsonPropertyName("lng")] public double? Lng { get; set; }
    [JsonPropertyName("erroGeocode")] public bool ErroGeocode { get; set; }
}
```

**Arquivo:** `FrontMenuSharedModels/Models/Roteirizacao/RotaOtimizada.cs`

```csharp
public class RotaOtimizada
{
    [JsonPropertyName("ordemOtimizada")] public List<int> OrdemOtimizada { get; set; } = new();
    [JsonPropertyName("distanciaKm")] public double DistanciaKm { get; set; }
    [JsonPropertyName("tempoMin")] public double TempoMin { get; set; }
}
```

**✔ Feito quando:** `dotnet build` do `FrontMenuSharedModels` passa sem erro.

---

## PASSO 3 — Service no Front

**Arquivo:** `FrontMenuWeb/Services/RoteirizacaoService.cs`

Seguir padrão de `CaixaEPagamentosService.cs`:

```csharp
public class RoteirizacaoService
{
    public HttpClient _HttpClient { get; set; }
    public RoteirizacaoService(HttpClient http) { _HttpClient = http; }

    public async Task<ReturnApiRefatored<PedidoParaRota>> GetPedidosDeliveryDoCaixaAsync()
    {
        var response = await _HttpClient.GetAsync("roteirizacao/pedidos-delivery-do-caixa");
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ReturnApiRefatored<PedidoParaRota>>(json, new() { PropertyNameCaseInsensitive = true })
            ?? new() { Status = "error", Messages = ["Erro ao buscar pedidos"] };
    }

    public async Task<ReturnApiRefatored<RotaOtimizada>> OtimizarRotaAsync(OtimizarRotaDto dto)
    {
        var response = await _HttpClient.PostAsJsonAsync("roteirizacao/otimizar", dto);
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ReturnApiRefatored<RotaOtimizada>>(json, new() { PropertyNameCaseInsensitive = true })
            ?? new() { Status = "error", Messages = ["Erro ao otimizar"] };
    }
}
```

**Registrar no DI:** `Program.cs` — `builder.Services.AddScoped<RoteirizacaoService>()` + `AddHttpClient<RoteirizacaoService>(...)` com o mesmo padrão usado pelos outros services autenticados.

**✔ Feito quando:** o service está registrado e pode ser injetado.

---

## PASSO 4 — JS do Mapa

**Arquivo:** `FrontMenuWeb/wwwroot/mapaRoteirizacao.js`

```js
window.mapaRoteirizacao = {
  mapa: null,
  markers: [],
  directionsRenderer: null,

  init: function(containerId, lat, lng) {
    this.mapa = new google.maps.Map(document.getElementById(containerId), {
      center: { lat, lng },
      zoom: 13
    });
    this.directionsRenderer = new google.maps.DirectionsRenderer({ map: this.mapa, suppressMarkers: true });
  },

  atualizarPontos: function(origem, pontos) {
    // Limpa markers anteriores
    this.markers.forEach(m => m.setMap(null));
    this.markers = [];

    // Marker de origem
    this.markers.push(new google.maps.Marker({
      position: origem, map: this.mapa, label: 'M', title: 'Merchant'
    }));

    // Markers dos pontos (numerados)
    pontos.forEach((p, i) => {
      this.markers.push(new google.maps.Marker({
        position: { lat: p.lat, lng: p.lng },
        map: this.mapa,
        label: String(i + 1),
        title: p.cliente
      }));
    });

    // Desenha linha conectando (simples polyline, sem directions)
    if (pontos.length > 0) {
      const path = [origem, ...pontos.map(p => ({ lat: p.lat, lng: p.lng }))];
      new google.maps.Polyline({ path, map: this.mapa, strokeColor: '#1976D2', strokeWeight: 3 });
    }
  }
};
```

**Adicionar ao `index.html`:** `<script src="https://maps.googleapis.com/maps/api/js?key=SUA_KEY"></script>` + `<script src="mapaRoteirizacao.js"></script>`. Key com restrição de HTTP referrer no Google Cloud Console.

**✔ Feito quando:** console do navegador não dá erro carregando o Maps.

---

## PASSO 5 — Página Blazor

**Arquivo:** `FrontMenuWeb/Pages/Vendas/RoteirizacaoDeEntregaPage.razor`

Rota: `/roteirizacao`. Estrutura:

```razor
@rendermode RenderMode.InteractiveWebAssembly
@attribute [Authorize]
@page "/roteirizacao"
@using FrontMenuWeb.Models.Roteirizacao
@using FrontMenuWeb.Services
@inject RoteirizacaoService RoteirizacaoService
@inject MotoboyService MotoboyService   // se não existir, criar similar a CaixaEPagamentosService
@inject IJSRuntime JS
@inject ISnackbar Snackbar
```

**Layout:** dois `MudDropContainer` (ou `MudDropZone` dentro de um `MudDropContainer<PedidoParaRota>`). Uma zona "Disponíveis", outra "Na rota" (esta ordenada).

**Propriedades:**
```csharp
private List<PedidoParaRota> Disponiveis = new();
private List<PedidoParaRota> NaRota = new();
private List<Motoboy> Motoboys = new();
private int? MotoboySelecionadoId;
private double DistanciaKm, TempoMin;
```

**OnInitializedAsync:**
1. Carregar pedidos via `RoteirizacaoService.GetPedidosDeliveryDoCaixaAsync()` → colocar em `Disponiveis`.
2. Carregar motoboys ativos.
3. Inicializar mapa: `await JS.InvokeVoidAsync("mapaRoteirizacao.init", "mapa-container", merchantLat, merchantLng);`

**Eventos:**
- Quando drop acontece: mover o item entre listas, chamar `AtualizarMapa()` que invoca `mapaRoteirizacao.atualizarPontos`.
- Botão "Otimizar rota": montar DTO, chamar `OtimizarRotaAsync`, reordenar `NaRota` conforme `OrdemOtimizada`, atualizar mapa, atualizar `DistanciaKm`/`TempoMin`.
- Botão "Imprimir": `window.print()` com um `@media print` no topo da página.
- Botão "Atribuir Tudo": loop em `NaRota` chamando `POST /motoboys/entregas`. Mostrar `MudProgressLinear`. No final, snackbar com resultado.

**Drag-and-drop:** usar `MudDropContainer<PedidoParaRota>` — ver documentação do MudBlazor. Implementar `ItemsSelector` pra dividir entre zonas por uma propriedade (ex: `EstaNaRota`).

**✔ Feito quando:**
- Abre a página, vê os pedidos delivery
- Arrasta um pra rota, pin aparece no mapa
- "Otimizar" reordena a lista e os pins
- "Imprimir" gera página imprimível
- "Atribuir Tudo" cria as entregas (verificar no banco)

---

## PASSO 6 — Estilo de impressão

Adicionar no `RoteirizacaoDeEntregaPage.razor` um bloco `<style>`:

```css
@media print {
  .no-print { display: none !important; }  /* botões, mapa, drag zones */
  .print-only { display: block !important; }
  body { background: white; }
}
@media screen {
  .print-only { display: none; }
}
```

Ter um `<div class="print-only">` com a lista numerada, distância/tempo, data, merchant.

---

## PASSO 7 — Smoke test

1. Com caixa aberto, 3 pedidos delivery com endereços válidos.
2. Abrir `/roteirizacao` → ver os 3 em "Disponíveis".
3. Arrastar 2 pra "Na rota" → pins aparecem no mapa.
4. Clicar "Otimizar" → ordem muda (ou mantém se já estava ótima), distância/tempo preenchem.
5. Selecionar motoboy, clicar "Atribuir" → verificar que `motoboys_entregas` tem os 2 registros novos.
6. `window.print` → preview mostra lista limpa.

---

## Pontos de atenção ao implementar

- **Google Maps key exposta**: a key do Maps JS fica exposta no HTML (normal), mas **com restrição de HTTP referrer** no Cloud Console. Geocoding e Routes são APIs server-side, key fica no `.env` do backend.
- **Cache de geocoding**: começar em memória (`Map` no service). Se ficar lento, migrar pra coluna no `Cliente`.
- **Atribuição ao motoboy**: endpoint é unitário. Se um falhar no loop, continuar os outros e somar erros pra mostrar no final.
- **Limite de 25 waypoints** no Routes API. Se tiver mais que isso na rota, mostrar aviso e desabilitar otimização.
- **Endereço ruim**: se `Geocode` retorna null, marcar `erroGeocode: true`. No front, o card não permite drag.
- **Fora do caixa aberto**: se não tem caixa, retornar erro (ou lista vazia) com mensagem clara.

---

## Perguntas que precisam ser respondidas antes/durante

1. A origem da rota é sempre o endereço do merchant? (assumir que sim — pegar de `merchant.endereco` e geocodar uma vez)
2. `ValorEntrega` ao atribuir ao motoboy: vem de `pedido.TaxaEntregaValor`? (assumir que sim; se for fixo por motoboy, buscar do `Motoboy`)
3. Filtrar só pedidos ainda não atribuídos a motoboy? (provavelmente sim — adicionar filtro no `GetPedidosDeliveryDoCaixaAtual`)
