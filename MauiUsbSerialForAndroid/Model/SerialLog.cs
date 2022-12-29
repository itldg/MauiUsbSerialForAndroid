using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiUsbSerialForAndroid.Model
{
    public class SerialLog
    {
        public string Data { get; set; }
        public bool IsSend { get; set; }
        public DateTime Time { get; set; }
        public string TimeString { get { return Time.ToString("HH:mm:ss:fff") + (IsSend ? "→" : "←"); } }
        public Color Color { get { return IsSend ? Colors.Green : Colors.Blue; } }
        public SerialLog(string data, bool isSend)
        {
            Data = data;
            IsSend = isSend;
            Time = DateTime.Now;
        }
    }
}
