
using MauiUsbSerialForAndroid.Helper;
using MauiUsbSerialForAndroid.View;
using MauiUsbSerialForAndroid.ViewModel;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

namespace MauiUsbSerialForAndroid;

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
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });


        builder.Services.AddSingleton<SerialPortHelper>();
        builder.Services.AddSingleton<SerialPortPage>();
        builder.Services.AddSingleton<SerialPortViewModel>();
        builder.Services.AddSingleton<SerialDataPage>();
        builder.Services.AddSingleton<SerialDataViewModel>();
#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
