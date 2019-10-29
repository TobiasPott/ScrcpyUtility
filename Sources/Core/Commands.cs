using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NoXP.Scrcpy
{
    public class Commands
    {
        public const string CMD_Help = "help";
        public const string CMD_Get = "get";
        public const string CMD_List = "list";
        public const string CMD_Select = "select";
        public const string CMD_Connect = "connect";
        public const string CMD_Reconnect = "reconnect";
        public const string CMD_Clear = "clear";
        public const string CMD_Quit = "quit";

        public const string CMD_SetMaxSize = "setMaxSize";
        public const string CMD_SetBitrate = "setBitrate";
        public const string CMD_SetNoControl = "setNoControl";
        public const string CMD_SetTurnScreenOff = "setTurnScreenOff";


        public static void RunShowHelp()
        {
            const int padding = 8;
            Console.WriteLine("Command-List:");
            Console.WriteLine("  " + CMD_Help.PadRight(padding) + "\t[Global]: This command, displays a list of available commands.");
            Console.WriteLine("  " + CMD_Get.PadRight(padding) + "\t[Global]: Gets all available devices (via ADB) and updates or retrieves their base information.");
            Console.WriteLine("  " + CMD_List.PadRight(padding) + "\t[Global]: Displays a list of all available devices and their base information and the connection state.");
            Console.WriteLine("  " + CMD_Select.PadRight(padding) + "\t[Global]: Selects a specific one of the available devices and makes it the current one all [Device] commands are run on.");
            Console.WriteLine("  " + CMD_Connect.PadRight(padding) + "\t[Device]: Connects scrcpy to the current device set by the '" + CMD_Select + "' command.");
            Console.WriteLine("  " + CMD_Clear.PadRight(padding) + "\t[Global]: Clears the current console screen.");
            Console.WriteLine("  " + CMD_Quit.PadRight(padding) + "\t[Global]: Terminates the application and all running connections to any available device started in this session.");
            Console.WriteLine();
        }

        public static void RunSelectDevice(bool auto = false)
        {
            // select device by index (this will be put into a loop later on)
            Commands.CLISelectDevice(auto);
            if (!auto)
            {
                if (ADBDevice.CurrentDevice != null)
                    Console.WriteLine(string.Format("Selected: {0}", ADBDevice.CurrentDevice));
                else
                    Console.WriteLine("No device selected.");
                Console.WriteLine();
            }
        }
        public static void RunConnectToDevice()
        {
            if (ADBDevice.CurrentDevice != null)
            {
                Console.WriteLine("Trying to connect to: {0}", ADBDevice.CurrentDevice);
                // override device's arguments with current global ones (e.g. update settings)
                ADBDevice.CurrentDevice.Connect(ScrcpyArguments.Global);
            }
            Console.WriteLine();
        }
        public static void RunReconnectToDevice()
        {
            if (ADBDevice.CurrentDevice != null)
            {
                Console.WriteLine("Trying to connect to: {0}", ADBDevice.CurrentDevice);
                // connect to device using the device's arguments (instead of global on connect)
                ADBDevice.CurrentDevice.Connect();
            }
            Console.WriteLine();
        }

        public static void RunGetAvailableDevices()
        {
            Commands.GetDevices();
            Console.WriteLine("Available devices updated.");
            Console.WriteLine();
        }
        public static void RunListAvailableDevices()
        {
            Console.WriteLine("Devices available on this machine:");
            if (ADBDevice.AllDevicesCollection.Count() > 0)
            {
                int i = 0;
                foreach (ADBDevice device in ADBDevice.AllDevicesCollection)
                {
                    char selectedMark = ' ';

                    if (device.IsConnected) Console.ForegroundColor = ConsoleColor.Green;
                    if (ADBDevice.CurrentDevice == device) selectedMark = '*';

                    Console.WriteLine("  " + string.Format(selectedMark + " [{0,3}] {1}", i, device));
                    Console.ResetColor();
                    i++;
                }
            }
            else
            {
                Console.WriteLine("  No devices connected. Connect an android device with enabled debugging and restart this application.");
                return;
            }
            Console.WriteLine();
        }
        public static void RunClear()
        {
            Console.ResetColor();
            Console.Clear();
        }



        private static void GetDevices()
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
        public static void GetDevicesIpAddress(ADBDevice device)
        {
            if (!string.IsNullOrEmpty(Constants.ADB)
                && device != null)
            {
                if (!GetDeviceIpAddressFromIpAddrShow(device))
                {
                    // do try to retrieve the ipv4 address via "ip addr show" command (remark: try order was reversed)
                    GetDeviceIpAddressFromIfconfig(device);
                }

            }

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
        private static void CLISelectDevice(bool auto = false)
        {
            string input = string.Empty;
            if (!auto)
            {
                Console.WriteLine("Enter the device's index:");
                Console.WriteLine("[leave blank to use first device in list]");
                Console.Write("> ");
                input = Console.ReadLine();
            }
            ADBDevice.SelectDevice(input, auto);
        }


        private static bool SetTCPIP()
        {
            if (!string.IsNullOrEmpty(Constants.ADB))
            {
                const string Restarting = "restarting ";
                string arguments = Constants.ADB_COMMAND_TCPIP + " " + Constants.ADB_TCPIP_PORT;

                Process proc = ProcessFactory.CreateProcessADB(arguments);
                proc.Start();

                string output = proc.StandardOutput.ReadToEnd().ToLowerInvariant();
                if (output.StartsWith(Restarting))
                    return true;
            }
            return false;
        }
        private static bool ConnectADBDeviceOverWifi(ADBDevice device)
        {
            if (!string.IsNullOrEmpty(Constants.ADB))
            {
                const string Connected = "connected ";

                string arguments = Constants.ADB_COMMAND_CONNECT + " " + device.IpAddress + ":" + Constants.ADB_TCPIP_PORT;

                Process proc = ProcessFactory.CreateProcessADB(arguments);
                proc.Start();

                string output = proc.StandardOutput.ReadToEnd().ToLowerInvariant();
                if (output.StartsWith(Connected))
                    return true;
            }
            return false;
        }

        //private static bool ConnectScrcpy(ADBDevice device, ScrcpyArguments arguments)
        //{
        //    if (!string.IsNullOrEmpty(Constants.SCRCPY))
        //    {
        //        Process proc = ProcessFactory.CreateProcessScrcpy(arguments.ToString());
        //        device.Process = proc;
        //        return proc.Start();
        //    }
        //    return false;
        //}

    }

}
