using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using TP6Programacion.Mobile.Services;

namespace TP6Programacion.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

		builder.Services.AddMudServices();
        builder.Services.AddSingleton<AlmacenamientoTokensService>();
        builder.Services.AddScoped(_ =>
        {
            var handler = new HttpClientHandler { UseCookies = false };

#if DEBUG && ANDROID
            handler.ServerCertificateCustomValidationCallback = (request, certificate, chain, errors) =>
            {
                if (errors == System.Net.Security.SslPolicyErrors.None)
                    return true;

                var uri = request.RequestUri;

                return uri?.Scheme == "https"
                    && uri.Host == "10.0.2.2"
                    && uri.Port == 7272
                    && certificate?.Issuer == "CN=localhost";
            };
#endif

            return new HttpClient(handler)
            {
                BaseAddress = new Uri(ConfiguracionApi.BaseUrl)
            };
        });
        builder.Services.AddScoped<AutenticacionService>();
        builder.Services.AddAuthorizationCore();
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<EstadoAutenticacionProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
            sp.GetRequiredService<EstadoAutenticacionProvider>());
        builder.Services.AddScoped<CatalogoService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
