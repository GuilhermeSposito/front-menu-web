# Guia Blazor Hybrid no SophosSyncDesktop

## Estrutura atual

```
SophosSyncDesktop/
├── BlazorComponents/
│   ├── _Imports.razor          ← usings globais + namespace
│   ├── App.razor               ← componente raiz (ponto de entrada)
│   └── Pages/
│       └── ImpressorasPage.razor
├── Views/
│   └── BlazorTestForm.cs       ← janela WinForms que hospeda o BlazorWebView
└── wwwroot/
    └── hybrid/
        └── index.html          ← página HTML que carrega o Blazor
```

---

## 1. Como abrir uma janela Blazor a partir do WinForms

Qualquer botão WinForms pode abrir o `BlazorTestForm` (ou uma variação dele):

```csharp
// Em qualquer Form WinForms
var btn = new Button { Text = "Pedidos" };
btn.Click += (s, e) => new BlazorTestForm().Show();
```

O `BlazorTestForm` já está configurado — ele cria o `BlazorWebView` e registra `App.razor` como componente raiz no `#app`.

---

## 2. Navegação entre páginas DENTRO do BlazorWebView

Para navegar sem abrir outra janela, use o **roteador do Blazor** (`Router`). Ele funciona igual ao Blazor Web — cada página tem uma rota `@page`.

### Passo 1 — Trocar `App.razor` pelo roteador

```razor
@namespace SophosSyncDesktop.BlazorComponents
@using Microsoft.AspNetCore.Components.Routing

<Router AppAssembly="typeof(App).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="routeData" DefaultLayout="typeof(MainLayout)" />
    </Found>
    <NotFound>
        <p>Página não encontrada.</p>
    </NotFound>
</Router>
```

### Passo 2 — Criar um layout base (opcional mas recomendado)

`BlazorComponents/Shared/MainLayout.razor`:

```razor
@inherits LayoutComponentBase

<div style="display:flex; height:100vh; font-family:'Segoe UI',sans-serif;">

    <!-- menu lateral -->
    <nav style="width:200px; background:#1e1e2e; padding:16px; display:flex; flex-direction:column; gap:8px;">
        <NavLink href="/" Match="NavLinkMatch.All"   style="color:white; text-decoration:none; padding:8px 12px; border-radius:6px;">
            Início
        </NavLink>
        <NavLink href="/impressoras"                  style="color:white; text-decoration:none; padding:8px 12px; border-radius:6px;">
            Impressoras
        </NavLink>
        <NavLink href="/pedidos"                      style="color:white; text-decoration:none; padding:8px 12px; border-radius:6px;">
            Pedidos
        </NavLink>
    </nav>

    <!-- conteúdo da página atual -->
    <main style="flex:1; overflow:auto;">
        @Body
    </main>

</div>
```

### Passo 3 — Adicionar `@page` em cada página

`BlazorComponents/Pages/ImpressorasPage.razor`:
```razor
@page "/impressoras"
@using System.Drawing.Printing

<!-- ... resto do componente ... -->
```

`BlazorComponents/Pages/PedidosPage.razor` (nova página):
```razor
@page "/pedidos"

<h2>Pedidos</h2>
<p>Conteúdo da página de pedidos.</p>
```

`BlazorComponents/Pages/HomePage.razor` (página inicial):
```razor
@page "/"

<h2>Bem-vindo</h2>
```

### Passo 4 — Adicionar o namespace de Shared no `_Imports.razor`

```razor
@namespace SophosSyncDesktop.BlazorComponents
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Routing
@using SophosSyncDesktop.BlazorComponents
@using SophosSyncDesktop.BlazorComponents.Pages
@using SophosSyncDesktop.BlazorComponents.Shared
@using System.Drawing.Printing
```

---

## 3. Navegar via código C# (sem clicar em link)

Injete `NavigationManager` no componente e chame `NavigateTo`:

```razor
@inject NavigationManager Nav

<button @onclick='() => Nav.NavigateTo("/pedidos")'>
    Ir para Pedidos
</button>
```

Ou de dentro do `@code`:

```razor
@code {
    void AbrirPedidos() => Nav.NavigateTo("/pedidos");
}
```

---

## 4. Passar dados entre WinForms → Blazor

Registre um serviço compartilhado no `ServiceCollection` do `BlazorTestForm`:

```csharp
// BlazorTestForm.cs
var services = new ServiceCollection();
services.AddWindowsFormsBlazorWebView();
services.AddSingleton<MeuContexto>();          // ← serviço compartilhado

var blazorWebView = new BlazorWebView { ... };
blazorWebView.Services = services.BuildServiceProvider();
```

```csharp
// MeuContexto.cs
public class MeuContexto
{
    public int MesaSelecionada { get; set; }
}
```

No WinForms, antes de abrir a janela:
```csharp
var ctx = new MeuContexto { MesaSelecionada = 5 };
var form = new BlazorTestForm(ctx);
form.Show();
```

No componente Blazor:
```razor
@inject MeuContexto Ctx

<p>Mesa: @Ctx.MesaSelecionada</p>
```

---

## Resumo rápido

| O que fazer | Como fazer |
|---|---|
| Abrir janela Blazor do WinForms | `new BlazorTestForm().Show()` |
| Navegar entre páginas | `Router` + `@page "/rota"` em cada `.razor` |
| Link de navegação | `<NavLink href="/rota">` ou `NavigationManager.NavigateTo` |
| Layout compartilhado (menu, header) | `LayoutComponentBase` em `MainLayout.razor` |
| Passar dados WinForms → Blazor | `services.AddSingleton<T>()` no `BlazorTestForm` |
