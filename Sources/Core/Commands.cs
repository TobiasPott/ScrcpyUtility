using System;
using System.Collections.Generic;
using System.Linq;


namespace NoXP.Scrcpy.Commands
{
    public class List : CommandBase<List>
    {
        public List() : base("list", CommandTypes.Global, "Displays a list of all available devices and their base information and the connection state.")
        { }
        public override void Execute(string args = "")
        {
            ADBDevice.GetAvailableDevices();
            Console.WriteLine("Devices available on this machine:");
            if (ADBDevice.AllDevicesCollection.Count() > 0)
            {
                // write table header to console
                string tableHeader = "  " + string.Format("   {0,3}  {1}", "#", string.Format(ADBDevice.FormatString, "Serial", "Mode", "State", "IP"));
                Console.WriteLine(tableHeader);
                Console.WriteLine(new String('=', tableHeader.Length));
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
    }
    public class Clear : CommandBase<Clear>
    {
        public Clear() : base("clear", CommandTypes.Global, "Clears the current console screen.")
        { }
        public override void Execute(string args = "")
        {
            Console.ResetColor();
            Console.Clear();
        }
    }
    public class Quit : CommandBase<Quit>
    {
        public Quit() : base("quit", CommandTypes.Global, "Terminates the application and all running connections to any available device started in this session.")
        { }
        public override void Execute(string args = "")
        {
            // terminate all scrcpy processes running for available devices
            foreach (ADBDevice device in ADBDevice.AllDevicesCollection)
            {
                if (device.IsConnected)
                {
                    device.Process.CloseMainWindow();
                    device.Process.Kill();
                }
            }
            // terminate the application
            Environment.Exit(0);
        }
    }
    public class Exit : Quit
    {
        public Exit() : base()
        {
            this.Instruction = "exit";
            CommandRegister.Register(this);
        }
    }
    public class Select : CommandBase<Select>
    {
        public Select() : base("select", CommandTypes.Global, "Selects a specific one of the available devices and makes it the current one all [Device] commands are run on.")
        { }
        public override void Execute(string args = "")
        {
            // select device by index (this will be put into a loop later on)
            string input = string.Empty;

            Console.WriteLine("Enter the device's index:");
            Console.WriteLine("[leave blank to use first device in list]");
            Console.Write("> ");
            input = Console.ReadLine();

            ADBDevice.SelectDevice(input);

            if (ADBDevice.CurrentDevice != null)
                Console.WriteLine(string.Format("Selected: {0}", ADBDevice.CurrentDevice));
            else
                Console.WriteLine("No device selected.");
            Console.WriteLine();
        }
    }
    public class Help : CommandBase<Help>
    {
        public Help() : base("help", CommandTypes.Global, "Displays a list of available commands.")
        { }
        public override void Execute(string args = "")
        {
            Console.WriteLine("Command-List");
            Console.WriteLine(" [Global]");
            IEnumerable<ICommand> globalCommands = CommandRegister.GetCommands(CommandTypes.Global);
            foreach (ICommand cmd in globalCommands)
                Console.WriteLine(string.Format("\t{0,-12}:\t{1}", cmd.Instruction, cmd.Help));
            Console.WriteLine();

            Console.WriteLine(" [Device]:");
            IEnumerable<ICommand> deviceCommands = CommandRegister.GetCommands(CommandTypes.Device);
            foreach (ICommand cmd in deviceCommands)
                Console.WriteLine(string.Format("\t{0,-12}:\t{1}", cmd.Instruction, cmd.Help));
            Console.WriteLine();

        }
    }


    public class Connect : CommandBase<Connect>
    {
        public Connect() : base("connect", CommandTypes.Device, "Connects scrcpy to the current device. Uses the global settings.")
        { }
        public override void Execute(string args = "")
        {
            if (ADBDevice.CurrentDevice != null)
            {
                Console.WriteLine("Trying to connect to: {0}", ADBDevice.CurrentDevice);
                // override device's arguments with current global ones (e.g. update settings)
                ADBDevice.CurrentDevice.Connect(ScrcpyArguments.Global);
            }
            Console.WriteLine();
        }
    }
    public class Reconnect : CommandBase<Reconnect>
    {
        public Reconnect() : base("reconnect", CommandTypes.Device, "Connects scrcpy to the current device. Uses the per-device settings.")
        { }
        public override void Execute(string args = "")
        {
            if (ADBDevice.CurrentDevice != null)
            {
                Console.WriteLine("Trying to connect to: {0}", ADBDevice.CurrentDevice);
                ADBDevice.CurrentDevice.Connect();
            }
            Console.WriteLine();
        }
    }
    public class Disconnect : CommandBase<Disconnect>
    {
        public Disconnect() : base("disconnect", CommandTypes.Device, "Disconnects scrcpy from the current device.")
        { }
        public override void Execute(string args = "")
        {
            if (ADBDevice.CurrentDevice != null)
            {
                Console.WriteLine("Trying to disconnect: {0}", ADBDevice.CurrentDevice);
                ADBDevice.CurrentDevice.Disconnect();
            }
            Console.WriteLine();
        }
    }
    public class ModeTCPIP : CommandBase<ModeTCPIP>
    {
        public ModeTCPIP() : base("mode-tcpip", CommandTypes.Device, "Changes mode to TCP/IP and allows a WiFi-connection to the current device.")
        { }
        public override void Execute(string args = "")
        {
            if (ADBDevice.CurrentDevice != null)
                ADBDevice.CurrentDevice.ConnectOverTCPIP();
        }
    }
    public class ModeUSB : CommandBase<ModeUSB>
    {
        public ModeUSB() : base("mode-usb", CommandTypes.Device, "Changes mode to USB and disables WiFi-connection to the current device.")
        { }
        public override void Execute(string args = "")
        {
            if (ADBDevice.CurrentDevice != null)
                ADBDevice.CurrentDevice.ConnectOverUSB();
        }
    }

}