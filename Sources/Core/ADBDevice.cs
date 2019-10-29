using System;
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
        public ScrcpyArguments Arguments { get; private set; }

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
            // assign global arguments on device creation
            this.SetScrcpyArguments(ScrcpyArguments.Global);
        }


        private void SetScrcpyArguments(ScrcpyArguments arguments = null)
        {
            if (arguments != null)
            {
                this.Arguments = arguments.Clone() as ScrcpyArguments;
                this.Arguments.Serial = this.Serial;
            }
        }

        public void Disconnect()
        {
            if (this.IsConnected)
                this.Process.Kill();
        }
        public bool Connect(ScrcpyArguments arguments = null)
        {
            if (!this.IsConnected && !string.IsNullOrEmpty(Constants.SCRCPY))
            {
                this.SetScrcpyArguments(arguments);
                Process proc = ProcessFactory.CreateProcessScrcpy(this.Arguments);
                this.Process = proc;
                return proc.Start();
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format("{0}: [{1}] \tIP:{2}", this.Serial, this.IsConnected ? "Connected" : "Disconnected", this.IpAddress);
        }



        public static void SelectDevice(string userInput, bool autoSelect = false)
        {
            // is user input is empty auto select is enabled
            if (string.IsNullOrEmpty(userInput))
                autoSelect = true;

            if (int.TryParse(userInput, out int deviceIndex))
                SelectDevice(deviceIndex, autoSelect);
            else
                SelectDevice(-1, autoSelect);
        }
        public static void SelectDevice(int index, bool autoSelect = false)
        {
            int currentDeviceIndex = index;
            if (currentDeviceIndex <= -1 && autoSelect && ADBDevice.NumberOfDevices > 0)
                currentDeviceIndex = 0;

            if (currentDeviceIndex < 0 || currentDeviceIndex >= ADBDevice.NumberOfDevices)
                currentDeviceIndex = -1;
            ADBDevice.SetCurrentDevice(currentDeviceIndex);
        }
        public static void SetCurrentDevice(int index)
        {
            if (index != -1 && index < ADBDevice.AllDevices.Count)
                _currentDevice = ADBDevice.AllDevices[index];
            else
                _currentDevice = null;
        }

        public static void GetAvailableDevices()
        {
            if (!string.IsNullOrEmpty(Constants.ADB))
            {
                Process proc = ProcessFactory.CreateProcessADB(Constants.ADB_COMMAND_DEVICES);
                proc.Start();

                string output = proc.StandardOutput.ReadToEnd();
                string[] lines = output.Split(Constants.SeparatorNewLine, StringSplitOptions.RemoveEmptyEntries);

                List<ADBDevice> newDevices = new List<ADBDevice>();
                if (lines.Length > 1)
                {
                    for (int i = 1; i < lines.Length; i++)
                    {
                        string[] deviceInfo = lines[i].Split(Constants.SeparatorWhitespace, 2);
                        newDevices.Add(new ADBDevice(deviceInfo[0], deviceInfo[1]));
                    }
                }
                ADBDevice.UpdateAllDevices(newDevices);
                // TODO:
                //  put this behind a configurable value to allow the user/application to decide whether auto-connect should be used or not
                // if only one device is available select it as current by default
                if (ADBDevice.NumberOfDevices == 1)
                    ADBDevice.SelectDevice(0);
            }

        }
        private static void UpdateAllDevices(List<ADBDevice> newDevices)
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
