namespace TP6Programacion.Mobile.Services;

public static class ConfiguracionApi
{
    // Sol, ahí te lo configure un poquito mejor para que puedas usar la URL del backend de la nube. Sumado a que te lo puse para que
    // se pueda contactar con el backend independientemente de si estás o no utilizando el emulador.
    public static string BaseUrl
    {
        get
        {
#if DEBUG
            return DeviceInfo.Platform == DevicePlatform.Android
                ? "https://10.0.2.2:7272/"
                : "https://localhost:7272/";
#else
            return "https://tpprogramacion-backend.runasp.net/";
#endif
        }
    }

}
