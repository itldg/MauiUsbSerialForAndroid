using Hoho.Android.UsbSerial.Driver;
using MauiUsbSerialForAndroid.ViewModel;
using System.Runtime.CompilerServices;

namespace MauiUsbSerialForAndroid.View;

public partial class SerialDataPage : ContentPage
{
    SerialDataViewModel vm;
    public SerialDataPage(SerialDataViewModel viewModel)
    {
        InitializeComponent();
        this.BindingContext = vm = viewModel;
    }

    private void Picker_SelectedIndexChanged(object sender, EventArgs e)
    {
        vm.SerialOptionChangeCommand.Execute(null);
    }
}