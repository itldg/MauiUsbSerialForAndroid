using Android.Widget;
using MauiUsbSerialForAndroid.Helper;
using MauiUsbSerialForAndroid.ViewModel;

namespace MauiUsbSerialForAndroid.View;

public partial class SerialPortPage : ContentPage
{
    SerialPortViewModel vm;
    public SerialPortPage(SerialPortViewModel vm)
    {
        InitializeComponent();
        BindingContext = this.vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!SerialPortHelper.UsbFeatureSupported())
        {
            await Shell.Current.DisplayAlert("fault", "Your device does not support USB", "ok");
            Environment.Exit(0);
            return;
        }
        vm.GetUsbDevices();
    }
}