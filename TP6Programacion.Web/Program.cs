using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using TP6Programacion.Web;
using TP6Programacion.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["Apis:TP6Programacion"] ?? throw new InvalidOperationException("Falta configurar Apis:TP6Programacion.")) });

builder.Services.AddMudServices();
builder.Services.AddScoped<AlmacenamientoTokensService>();
builder.Services.AddScoped<AutenticacionService>();

await builder.Build().RunAsync();
