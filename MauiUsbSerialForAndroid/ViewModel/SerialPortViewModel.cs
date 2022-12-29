using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiUsbSerialForAndroid.Helper;
using System.Collections.ObjectModel;
using Hoho.Android.UsbSerial.Driver;
using Android.Hardware.Usb;
using CommunityToolkit.Mvvm.Input;
using MauiUsbSerialForAndroid.Model;
using MauiUsbSerialForAndroid.View;
using Android.OS;
using Android.Content;
using Android.Util;

namespace MauiUsbSerialForAndroid.ViewModel
{
    [ObservableObject]
    public partial class SerialPortViewModel : IQueryAttributable
    {

        bool openIng = false;
        public ObservableCollection<UsbDeviceInfo> UsbDevices { get; } = new();
        public SerialPortViewModel()
        {
            SerialPortHelper.WhenUsbDeviceAttached((usbDevice) =>
            {
                GetUsbDevices();
            });

            SerialPortHelper.WhenUsbDeviceDetached((usbDevice) =>
            {
                GetUsbDevices();
            });
        }
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            GetUsbDevices();
        }
        [RelayCommand]
        public async Task GetUsbDevices()
        {
            UsbDevices.Clear();
            var list = SerialPortHelper.GetUsbDevices();
            foreach (var item in list)
            {
                UsbDevices.Add(item);
                //fix VirtualView cannot be null here
                await Task.Delay(50);
            }
        }
        [RelayCommand]
        async Task Open(UsbDeviceInfo usbDeviceInfo)
        {
            if (openIng) { return; }
            openIng = true;
            await Shell.Current.GoToAsync(nameof(SerialDataPage), new Dictionary<string, object> {
                    { "Serial",usbDeviceInfo}
                });
            openIng = false;
        }

    }
}
