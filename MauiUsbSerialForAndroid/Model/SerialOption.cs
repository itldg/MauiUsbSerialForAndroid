using CommunityToolkit.Mvvm.ComponentModel;
using Hoho.Android.UsbSerial.Driver;

namespace MauiUsbSerialForAndroid.Model
{
    [ObservableObject]
    public partial class SerialOption
    {
        [ObservableProperty]
        int baudRate = 9600;
        [ObservableProperty]
        int dataBits = 8;
        [ObservableProperty]
        string stopBitsName = StopBits.One.ToString();
        public StopBits StopBits => Enum.Parse<StopBits>(StopBitsName);
        [ObservableProperty]
        string parityName = Parity.None.ToString();
        public Parity Parity => Enum.Parse<Parity>(ParityName);
    }
}
