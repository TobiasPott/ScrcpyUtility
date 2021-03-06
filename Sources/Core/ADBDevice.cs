﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NoXP.Scrcpy
{
    public class ADBDevice
    {
        private static AllocationPool<int> _ports = new AllocationPool<int>(Enumerable.Range(5555, 64));

        private static List<ADBDevice> AllDevices { get; } = new List<ADBDevice>();

        public static IEnumerable<ADBDevice> AllDevicesCollection { get => AllDevices; }

        public static ADBDevice CurrentDevice { get; private set; } = null;
        public static int NumberOfDevices
        { get => AllDevices.Count; }


        public string Serial { get; }
        public string Name { get; }
        public string IpAddress { get; set; }

        public bool TCPIPEnabled { get; set; } = false;
        public int Port { get; set; } = -1;


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
                //this.Arguments.Serial = this.Serial;
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
                Process proc = ProcessFactory.CreateProcessScrcpy(this.Arguments, this.TCPIPEnabled ? this.IpAddress : this.Serial);
                this.Process = proc;
                return proc.Start();
            }
            return false;
        }
        public void GetIpAddress()
        {
            if (!string.IsNullOrEmpty(Constants.ADB))
            {
                if (!ADBDevice.GetDeviceIpAddressFromIpAddrShow(this))
                    // do try to retrieve the ipv4 address via "ip addr show" command (remark: try order was reversed)
                    ADBDevice.GetDeviceIpAddressFromIfconfig(this);
            }
        }


        public bool ConnectOverTCPIP()
        {
            if (this.SetTCPIPMode())
            {
                if (this.ConnectADBOverTCPIP())
                    return true;
                else
                    this.SetUSBMode();
            }
            return false;
        }
        public bool ConnectOverUSB()
        {
            if (this.DisconnectADBOverTCPIP())
            {
                if (this.SetUSBMode())
                    return true;
            }
            return false;
        }
        private bool SetTCPIPMode()
        {
            if (!string.IsNullOrEmpty(Constants.ADB))
            {
                if (this.Port == -1)
                    this.Port = ADBDevice._ports.Allocate();

                const string Restarting = "restarting ";
                string arguments = " -s " + this.Serial + " " + Constants.ADB_COMMAND_TCPIP + " " + this.Port;

                Process proc = ProcessFactory.CreateProcessADB(arguments);
                proc.Start();

                string output = proc.StandardOutput.ReadToEnd().ToLowerInvariant();
                if (output.StartsWith(Restarting))
                {
                    this.TCPIPEnabled = true;
                    return true;
                }
            }
            return false;
        }
        private bool SetUSBMode()
        {
            if (!string.IsNullOrEmpty(Constants.ADB))
            {
                const string Error = "error";
                string arguments = " -s " + this.Serial + " " + Constants.ADB_COMMAND_USB;

                Process proc = ProcessFactory.CreateProcessADB(arguments);
                proc.Start();

                string output = proc.StandardOutput.ReadToEnd().ToLowerInvariant();
                if (!output.ToLowerInvariant().Contains(Error))
                {
                    this.Port = -1;
                    this.TCPIPEnabled = false;
                    return true;
                }
            }
            return false;
        }
        private bool ConnectADBOverTCPIP()
        {
            if (!string.IsNullOrEmpty(Constants.ADB) && this.TCPIPEnabled)
            {
                const string Connected = "connected ";
                string arguments = Constants.ADB_COMMAND_CONNECT + " " + this.IpAddress + ":" + this.Port;

                Process proc = ProcessFactory.CreateProcessADB(arguments);
                proc.Start();

                string output = proc.StandardOutput.ReadToEnd().ToLowerInvariant();
                if (output.StartsWith(Connected))
                    return true;
            }
            return false;
        }
        private bool DisconnectADBOverTCPIP()
        {
            if (!string.IsNullOrEmpty(Constants.ADB) && this.TCPIPEnabled)
            {
                const string Disconnected = "disconnected ";
                string arguments = Constants.ADB_COMMAND_DISCONNECT + " " + this.IpAddress + ":" + this.Port;

                Process proc = ProcessFactory.CreateProcessADB(arguments);
                proc.Start();

                string output = proc.StandardOutput.ReadToEnd().ToLowerInvariant();
                if (output.StartsWith(Disconnected))
                    return true;
            }
            return false;
        }


        public const string FormatString = "{0,-22} {1,-10} {2,-15} {3,-18}";
        public override string ToString()
        {
            return string.Format(ADBDevice.FormatString,
                this.Serial,
                this.TCPIPEnabled ? "TCP/IP" : "USB",
                this.IsConnected ? "Connected" : "Disconnected",
                this.IpAddress);
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
                CurrentDevice = ADBDevice.AllDevices[index];
            else
                CurrentDevice = null;
        }

        // TODO:
        //  filter devices which are available due to their connection via TCPIP
        //      ->  either remove them from the list completely
        //      ->  or group/combine them with other devices which share the same IP-Address
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
                device.GetIpAddress();

        }


        private static bool GetDeviceIpAddressFromIfconfig(ADBDevice device)
        {

            if (!string.IsNullOrEmpty(Constants.ADB))
            {
                const string InetAddr = "inet addr:";
                const string Bcast = "Bcast:";
                const string Denied = "denied";
                const string Wlan = "wlan";

                string arguments = string.Empty;
                arguments += string.Format("-s {0} ", device.Serial);
                arguments += Constants.ADB_COMMAND_SHELL_IFCONFIG;

                Process proc = ProcessFactory.CreateProcessADB(arguments);
                proc.Start();

                string output = proc.StandardOutput.ReadToEnd().ToLowerInvariant();
                // check if the result message contains anything with denied in it
                if (output.Contains(Denied))
                    return false;

                int indexOfWlan = output.IndexOf(Wlan);
                int indexOfInetAddr = -1;
                int indexOfBcast = -1;

                if (indexOfWlan != -1)
                {
                    // get index of 'Bcast:' string value
                    indexOfBcast = output.IndexOf(Bcast, indexOfWlan);
                    // get index of 'inet addr:' string value
                    indexOfInetAddr = output.IndexOf(InetAddr, indexOfWlan);
                    if (indexOfInetAddr != -1)
                        indexOfInetAddr += InetAddr.Length;

                    // only try retrieve the ip address when the indices are not -1
                    if (indexOfInetAddr != -1 && indexOfBcast != -1)
                    {
                        string ipAddress = output.Substring(indexOfInetAddr, indexOfBcast - indexOfInetAddr).TrimEnd();
                        device.IpAddress = ipAddress;
                        return true;
                    }
                }

            }
            return false;
        }
        private static bool GetDeviceIpAddressFromIpAddrShow(ADBDevice device)
        {
            // ! ! ! !
            // convert processing of output/input stream to use the event receiver method to hide it's output on the apps console
            // https://stackoverflow.com/questions/43668920/stop-process-output-from-being-displayed-in-project-command-window
            // ! ! ! !
            if (!string.IsNullOrEmpty(Constants.ADB))
            {
                const string Inet = "inet";
                const string Denied = "denied";
                string ifNameBase = "wlan"; // append interface index to name to iterate over possible available wireless interfaces


                for (int i = 0; i < 4; i++)
                {
                    string arguments = string.Empty;
                    arguments += string.Format("-s {0} ", device.Serial);
                    string ifName = ifNameBase + i.ToString();
                    arguments += string.Format(Constants.ADB_COMMAND_SHELL_IPADDRSHOW, ifName);

                    Process proc = ProcessFactory.CreateProcessADB(arguments);
                    proc.Start();

                    string output = proc.StandardOutput.ReadToEnd().ToLowerInvariant();
                    // check if the result message contains anything with denied in it
                    if (output.Contains(Denied))
                        continue;

                    int indexOfInet = output.IndexOf(Inet);
                    int indexOfIfName = -1;

                    if (indexOfInet != -1)
                    {
                        if (indexOfInet != -1)
                            indexOfInet += Inet.Length;

                        // get index of 'inet' string value
                        indexOfIfName = output.IndexOf(ifName, indexOfInet);
                        if (indexOfIfName != -1)
                        {
                            string rawLine = output.Substring(indexOfInet, indexOfIfName - indexOfInet).TrimEnd();
                            int indexOfMask = rawLine.IndexOf('/');
                            if (indexOfMask != -1)
                            {
                                device.IpAddress = rawLine.Substring(0, indexOfMask).Trim();
                                return true;
                            }
                        }
                    }

                }
            }
            return false;
        }
    }

}
