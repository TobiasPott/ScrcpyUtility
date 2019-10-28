using System.Collections.Generic;
using System.Diagnostics;

namespace NoXP.Scrcpy
{
    public class ADBDevice
    {
        private static ADBDevice _currentDevice = null;
        private static List<ADBDevice> AllDevices { get; } = new List<ADBDevice>();

        public static IEnumerable<ADBDevice> AllDevicesCollection { get => AllDevices; }

        public static ADBDevice CurrentDevice
        { get => _currentDevice; }
        public static int NumberOfDevices
        { get => AllDevices.Count; }


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


        public void Disconnect()
        {
            if (this.IsConnected)
                this.Process.Kill();
        }
        public override string ToString()
        {
            return string.Format("{0}: [{1}] \tIP:{2}", this.Serial, this.IsConnected ? "Connected" : "Disconnected", this.IpAddress);
        }

        public static void SetCurrentDevice(int index)
        {
            if (index != -1 && index < ADBDevice.AllDevices.Count)
                _currentDevice = ADBDevice.AllDevices[index];
            else
                _currentDevice = null;
        }
        public static void UpdateAllDevices(List<ADBDevice> newDevices)
        {
            for (int i = AllDevices.Count - 1; i >= 0; i--)
            {
                ADBDevice device = ADBDevice.AllDevices[i];
                int indexOfAvailable = newDevices.FindIndex((x) => x.Serial.Equals(device.Serial));
                if (indexOfAvailable == -1)
                {
                    device.Disconnect();
                    ADBDevice.AllDevices.Remove(device);
                }
                else
                {
                    // remove the available device from the new device list to avoid adding it again
                    newDevices.RemoveAt(indexOfAvailable);
                }
            }
            // add all remaining devices to the all devices list
            foreach (ADBDevice device in newDevices)
                ADBDevice.AllDevices.Add(device);

            // update device data of all available devices
            foreach (ADBDevice device in AllDevices)
                Commands.GetDevicesIpAddress(device);

        }

    }

}
