using System.Collections.Generic;
using System.Diagnostics;

namespace NoXP.Scrcpy
{
    public class ADBDevice
    {
        public static List<ADBDevice> AllDevices { get; } = new List<ADBDevice>();



        public string Serial { get; }
        public string Name { get; }
        public string IpAddress { get; set; }

        public Process Process { get; set; }

        public bool IsConnected
        {
            get
            {
                if (this.Process == null)
                    return false;
                return !this.Process.HasExited;
            }
        }

        public ADBDevice(string serial, string name)
        {
            this.Serial = serial;
            this.Name = name;
        }

        public override string ToString()
        {
            return string.Format("{0}: [{1}] \tIP:{2}", this.Serial, this.IsConnected ? "Connected" : "Disconnected", this.IpAddress);
        }



    }

}
