namespace TP6Programacion.Mobile.Services;

public static class ConfiguracionApi
{
    // En dispositivos físicos, usar la URL HTTPS del backend accesible desde el dispositivo.
    public static string BaseUrl => DeviceInfo.Platform == DevicePlatform.Android
        ? "https://10.0.2.2:7272/"
        : "https://localhost:7272/";
}
