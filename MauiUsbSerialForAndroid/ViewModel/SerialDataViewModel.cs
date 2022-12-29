using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiUsbSerialForAndroid.Helper;
using MauiUsbSerialForAndroid.Model;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace MauiUsbSerialForAndroid.ViewModel
{
    [ObservableObject]
    public partial class SerialDataViewModel : IQueryAttributable
    {
        [ObservableProperty]
        bool isOpen = false;
        public UsbDeviceInfo DeviceInfo { get; set; }

        public string[] AllEncoding { get; } = new string[] { "HEX", "ASCII", "UTF-8", "GBK", "GB2312", "Unicode" };
        [ObservableProperty]
        string encodingSend = "HEX";
        [ObservableProperty]
        string encodingReceive = "HEX";
        [ObservableProperty]
        int intervalReceive = 50;
        [ObservableProperty]
        int intervalSend = 50;
        [ObservableProperty]
        bool cycleToSend = false;
        [ObservableProperty]
        bool showTimeStamp = true;
        [ObservableProperty]
        bool autoScroll = true;
        [ObservableProperty]
        string sendData = "";


        public ObservableCollection<SerialLog> Datas { get; } = new();
        System.Timers.Timer timerSend;
        public SerialDataViewModel()
        {
            SerialPortHelper.WhenDataReceived().Subscribe(data =>
            {
                string text = SerialPortHelper.GetData(data, EncodingReceive);
                AddLog(new SerialLog(text, false));
            });
            timerSend = new System.Timers.Timer(intervalSend);
            timerSend.Elapsed += TimerSend_Elapsed;
            timerSend.Enabled = false;

            SerialPortHelper.WhenUsbDeviceAttached((usbDevice) =>
            {
                if (usbDevice.DeviceId == DeviceInfo.Device.DeviceId)
                {
                    AddLog(new SerialLog("Usb device attached", false));
                    Open();
                }
            });

            SerialPortHelper.WhenUsbDeviceDetached((usbDevice) =>
            {
                if (usbDevice.DeviceId == DeviceInfo.Device.DeviceId)
                {
                    AddLog(new SerialLog("Usb device detached", false));
                    Close();
                }
            });
        }

        private void TimerSend_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Send();
        }

        partial void OnIntervalReceiveChanged(int value)
        {
            SerialPortHelper.IntervalChange(value);
        }

        async partial void OnIntervalSendChanged(int value)
        {
            if (value < 5)
            {
                await Shell.Current.DisplayAlert("TooFast", "Set at least 5 milliseconds", "ok");
                IntervalSend = 5;
            }
            timerSend.Interval = IntervalSend;
        }
        partial void OnCycleToSendChanged(bool value)
        {
            timerSend.Enabled = value;
        }
        Regex regHexRemove = new Regex("[^a-fA-F0-9 ]");
        partial void OnSendDataChanged(string value)
        {
            if (EncodingSend == "HEX")
            {
                string temp = regHexRemove.Replace(value, "");
                if (SendData != temp)
                {
                    Shell.Current.DisplayAlert("Only hex", "Only HEX characters can be entered", "Ok");
                    SendData = temp;
                }
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("Serial"))
            {
                DeviceInfo = (UsbDeviceInfo)query["Serial"];
                Open();
            }
        }

        [RelayCommand]
        public void Toggle()
        {
            if (IsOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }
        [RelayCommand]
        public async void Open()
        {
            if (!IsOpen)
            {
                string r = await SerialPortHelper.RequestPermissionAsync(DeviceInfo);
                if (SerialPortHelper.CheckError(r, showDialog: false))
                {
                    r = SerialPortHelper.Open(DeviceInfo);
                    if (SerialPortHelper.CheckError(r, showDialog: false))
                    {
                        IsOpen = true;
                    }
                    else
                    {
                        AddLog(new SerialLog(r, false));
                    }
                }
                else
                {
                    AddLog(new SerialLog(r, false));
                }
            }
        }
        [RelayCommand]
        public void Close()
        {
            try
            {
                SerialPortHelper.Close();
                CycleToSend = false;
                IsOpen = false;
            }
            catch (Exception)
            {
            }

        }
        [RelayCommand]
        public void Clear()
        {
            Datas.Clear();
        }
        [RelayCommand]
        public void Send()
        {
            byte[] send = SerialPortHelper.GetBytes(SendData, EncodingSend);
            if (send.Length == 0)
            {
                return;
            }
            string r = SerialPortHelper.Write(send);
            if (SerialPortHelper.CheckError(r))
            {
                if (EncodingSend == "HEX")
                {
                    AddLog(new SerialLog(SendData.ToUpper(), true));
                }
                else
                {
                    AddLog(new SerialLog(SendData, true));
                }

            }
            else
            {
                AddLog(new SerialLog(r, true));
            }

        }
        [RelayCommand]
        public async void Back()
        {
            Close();
            await Shell.Current.GoToAsync("..");
        }

        void AddLog(SerialLog serialLog)
        {
            Datas.Add(serialLog);
            //fix VirtualView cannot be null here
            Task.Delay(50);
        }
    }
}
