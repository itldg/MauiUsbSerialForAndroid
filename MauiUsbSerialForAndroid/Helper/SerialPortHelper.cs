using Android.App;
using Android.Content;
using Android.Hardware;
using Android.Hardware.Usb;
using Android.Runtime;
using Android.Util;
using CommunityToolkit.Mvvm.ComponentModel;
using Hoho.Android.UsbSerial.Driver;
using Hoho.Android.UsbSerial.Extensions;
using Hoho.Android.UsbSerial.Util;
using MauiUsbSerialForAndroid.Model;
using Org.Xmlpull.V1.Sax2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace MauiUsbSerialForAndroid.Helper
{
    [ObservableObject]
    public partial class SerialPortHelper
    {
        static Context context => Android.App.Application.Context;
        static UsbManager usbManager = (UsbManager)context.GetSystemService(Context.UsbService);
        private static UsbSerialPort _port;
        private static SerialInputOutputManager serialIoManager;
        static System.Timers.Timer timerData;
        static List<byte> listByteCache = new();
        static int interval = 50;
        static Subject<byte[]> dataSubject = new Subject<byte[]>();
        const int WRITE_WAIT_MILLIS = 1000;
        public static IObservable<byte[]> WhenDataReceived() => dataSubject;

        /// <summary>
        /// 系统是否支持USB Host功能
        /// </summary>
        /// <returns>true:系统支持USB Host false:系统不支持USB Host</returns>
        public static bool UsbFeatureSupported()
        {
            bool r = context.PackageManager.HasSystemFeature("android.hardware.usb.host");
            return r;
        }
        public static List<UsbDeviceInfo> GetUsbDevices()
        {
            var drivers = UsbSerialProber.GetDefaultProber().FindAllDrivers(usbManager);
            List<UsbDeviceInfo> r = new List<UsbDeviceInfo>();
            foreach (var item in usbManager.DeviceList.Values)
            {
                IUsbSerialDriver usbSerialDriver = drivers.Where(x => x.Device.DeviceId == item.DeviceId).FirstOrDefault();
                r.Add(new UsbDeviceInfo()
                {
                    Device = item,
                    Driver = usbSerialDriver,
                    DriverName = usbSerialDriver == null ? "" : usbSerialDriver.GetType().Name.Replace("SerialDriver", "")
                }); ;
            }
            return r;
        }
        public static async Task<string> RequestPermissionAsync(UsbDeviceInfo usbDeviceInfo)
        {
            if (usbDeviceInfo.Driver == null)
            {
                return "No driver";
            }
            if (!await usbManager.RequestPermissionAsync(usbDeviceInfo.Device, context))
            {
                return "Request permission failed";
            }
            return "";
        }

        public static string Open(UsbDeviceInfo usbDeviceInfo)
        {
            timerData?.Stop();
            timerData = new System.Timers.Timer(interval);
            timerData.Enabled = false;
            timerData.AutoReset = false;
            timerData.Elapsed += TimerData_Elapsed; ;
            UsbDeviceConnection connection = usbManager.OpenDevice(usbDeviceInfo.Device);
            if (connection == null)
            {
                return "Connection falut";
            }
            _port = usbDeviceInfo.Driver.Ports.FirstOrDefault();
            if (_port == null)
            {
                return "No port";
            }
            serialIoManager = new SerialInputOutputManager(_port)
            {
                BaudRate = 19200,
                DataBits = 8,
                StopBits = StopBits.One,
                Parity = Parity.None,
            };
            serialIoManager.DataReceived += SerialIoManager_DataReceived;
            try
            {
                serialIoManager.Open(usbManager);

            }
            catch (Exception ex)
            {

                return ex.Message;
            }
            return "";
        }

        private static void TimerData_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            cacheDataSend();
        }

        private static void SerialIoManager_DataReceived(object sender, SerialDataReceivedArgs e)
        {
            if (interval == 0)
            {
                dataSubject.OnNext(e.Data);
            }
            else
            {
                listByteCache.AddRange(e.Data);
                if (listByteCache.Count > 4098)
                {
                    timerData.Stop();
                    cacheDataSend();
                }
                else
                {
                    timerData.Stop();
                    timerData.Start();
                }

            }
        }
        static void cacheDataSend()
        {
            byte[] bytes = listByteCache.ToArray();
            listByteCache.Clear();
            dataSubject.OnNext(bytes);
        }
        public static void Close()
        {
            _port?.Close();
        }
        public static string Write(byte[] data)
        {
            try
            {
                if (serialIoManager.IsOpen)
                {
                    _port.Write(data, WRITE_WAIT_MILLIS);
                    return "";
                }
                else
                {
                    return "Serial port is not open";
                }
            }
            catch (Exception ex)
            {
                return "error:" + ex.Message;
            }
        }
        public static void IntervalChange(int interval)
        {
            if (timerData != null)
            {
                timerData.Interval = interval;
            }
        }

        public static bool CheckError(string error, string title = "fault", bool showDialog = true)
        {
            if (string.IsNullOrEmpty(error))
            {
                return true;
            }
            if (showDialog)
            {
                Shell.Current.DisplayAlert(title, error, "OK");
            }
            return false;
        }
        public static string GetData(byte[] data, string encodingName)
        {
            if (encodingName.ToUpper() == "HEX")
            {
                return ByteToHex(data);
            }
            else
            {
                return Encoding.GetEncoding(encodingName).GetString(data);
            }
        }

        public static byte[] GetBytes(string data, string encodingName)
        {
            if (encodingName.ToUpper() == "HEX")
            {
                return HexToByte(data);
            }
            else
            {
                return Encoding.GetEncoding(encodingName).GetBytes(data);
            }
        }

        public static byte[] HexToByte(string msg)
        {
            msg = msg.Replace(" ", "");
            if (msg.Length % 2 != 0)
            {
                msg = "0" + msg;
            }
            byte[] comBuffer = new byte[msg.Length / 2];
            for (int i = 0; i < msg.Length; i += 2)
            {
                comBuffer[i / 2] = (byte)Convert.ToByte(msg.Substring(i, 2), 16);
            }
            return comBuffer;
        }
        public static string ByteToHex(byte[] comByte)
        {
            StringBuilder sb = new StringBuilder();
            if (comByte != null)
            {
                for (int i = 0; i < comByte.Length; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(" ");
                    }
                    sb.Append(comByte[i].ToString("X2"));
                }
            }
            return sb.ToString();
        }
        public static void WhenUsbDeviceDetached(Action<UsbDevice> action)
        {
            var detachedReceiver = new UsbDeviceDetachedReceiver(action);
            context.RegisterReceiver(detachedReceiver, new IntentFilter(UsbManager.ActionUsbDeviceDetached));
        }
        public static void WhenUsbDeviceAttached(Action<UsbDevice> action)
        {
            var attachedReceiver = new UsbDeviceAttachedReceiver(action);
            context.RegisterReceiver(attachedReceiver, new IntentFilter(UsbManager.ActionUsbDeviceAttached));
        }
    }
    class UsbDeviceDetachedReceiver
           : BroadcastReceiver
    {
        readonly string TAG = typeof(UsbDeviceDetachedReceiver).Name;
        readonly Action<UsbDevice> action;

        public UsbDeviceDetachedReceiver(Action<UsbDevice> action)
        {
            this.action = action;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            var device = (UsbDevice)intent.GetParcelableExtra(UsbManager.ExtraDevice);
            Log.Info(TAG, "USB device detached: " + device.DeviceName);
            action(device);
        }
    }

    class UsbDeviceAttachedReceiver
        : BroadcastReceiver
    {
        readonly string TAG = typeof(UsbDeviceAttachedReceiver).Name;
        readonly Action<UsbDevice> action;

        public UsbDeviceAttachedReceiver(Action<UsbDevice> action)
        {
            this.action = action;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            var device = intent.GetParcelableExtra(UsbManager.ExtraDevice) as UsbDevice;
            Log.Info(TAG, "USB device attached: " + device.DeviceName);
            action(device);
        }
    }
}
