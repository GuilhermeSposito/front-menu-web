using Microsoft.AspNetCore.Components.WebAssembly.Http;

internal class BrowserRequestCredentialsMessageHandler : DelegatingHandler
{
    private BrowserRequestCredentials include;

    public BrowserRequestCredentialsMessageHandler(BrowserRequestCredentials include)
    {
        this.include = include;
    }
}