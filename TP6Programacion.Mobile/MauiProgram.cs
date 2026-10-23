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
        builder.Services.AddScoped(_ => new HttpClient(new HttpClientHandler { UseCookies = false })
        {
            BaseAddress = new Uri(ConfiguracionApi.BaseUrl)
        });
        builder.Services.AddScoped<AutenticacionService>();
        builder.Services.AddScoped<CatalogoService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
