using Android.Hardware.Usb;
using Hoho.Android.UsbSerial.Driver;

namespace MauiUsbSerialForAndroid.Model
{
    public class UsbDeviceInfo
    {
        public UsbDevice Device { get; set; }
        public IUsbSerialDriver Driver { get; set; }
        public string DriverName { get; set; }
    }
}
