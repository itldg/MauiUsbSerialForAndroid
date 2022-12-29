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

    private void lblCycleToSend_Tapped(object sender, TappedEventArgs e)
    {
        chkCycleToSend.IsChecked = !chkCycleToSend.IsChecked;
    }
}