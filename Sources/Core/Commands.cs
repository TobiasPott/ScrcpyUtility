using System;
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

        // untested
        public const string CMD_ADBMode_TCPIP = "adbmode-tcpip";
        public const string CMD_ADBMode_USB = "adbmode-usb";

        // not implemented
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
            string input = string.Empty;
            if (!auto)
            {
                Console.WriteLine("Enter the device's index:");
                Console.WriteLine("[leave blank to use first device in list]");
                Console.Write("> ");
                input = Console.ReadLine();
            }
            ADBDevice.SelectDevice(input, auto);

            if (ADBDevice.CurrentDevice != null)
                Console.WriteLine(string.Format("Selected: {0}", ADBDevice.CurrentDevice));
            else
                Console.WriteLine("No device selected.");
            Console.WriteLine();
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
            ADBDevice.GetAvailableDevices();
            RunListAvailableDevices();
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




        public static void RunADBModeTCPIP()
        {
            if (ADBDevice.CurrentDevice != null)
            {
                ADBDevice.CurrentDevice.SetTCPIPMode();
                ADBDevice.CurrentDevice.ConnectADBDeviceOverWifi();
            }
        }
        public static void RunADBModeUSB()
        {
            if (ADBDevice.CurrentDevice != null)
            {
                ADBDevice.CurrentDevice.DisconnectADBDeviceOverWifi();
                ADBDevice.CurrentDevice.SetUSBMode();
            }
        }
    }

}
