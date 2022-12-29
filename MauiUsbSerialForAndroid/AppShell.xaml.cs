using MauiUsbSerialForAndroid.View;
namespace MauiUsbSerialForAndroid;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(SerialPortPage), typeof(SerialPortPage));
        Routing.RegisterRoute(nameof(SerialDataPage), typeof(SerialDataPage));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
    }
}
