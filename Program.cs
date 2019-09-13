using System;
using System.Runtime.InteropServices;

namespace NoXP.Scrcpy
{
    internal class Program
    {

        private static string _command = "";

        private static void Main(string[] args)
        {
            Console.BufferWidth = Math.Min(Console.LargestWindowWidth, 150);
            Console.WindowWidth = Math.Min(Console.LargestWindowWidth, 150);

            if (args.Length > 0)
                ProcessFactory.BasePath = args[0];
            else
                ProcessFactory.BasePath = Program.GetFallbackPath();

            Console.WriteLine("Utility is using '" + ProcessFactory.BasePath + "' as the path to your scrcpy-installation.");

            // preset the scrcpy arguments with some default values
            Commands.Arguments.NoControl = false;
            Commands.Arguments.TurnScreenOff = true;
            Commands.Arguments.MaxSize = 1280;
            // prefills the list of devices available on this machine
            Commands.RunGetAvailableDevices();
            Commands.RunSelectDevice(true);

            do
            {
                Console.WriteLine("Please enter your command (type 'quit' to terminate the application).");
                Console.Write("> ");

                _command = Console.ReadLine();
                Program.ProcessCommand(_command);
            }
            while (!_command.Equals(Commands.CMD_Quit));

            // terminate all scrcpy processes running for available devices
            foreach (ADBDevice device in ADBDevice.AllDevices)
            {
                if (device.IsConnected)
                {
                    device.Process.CloseMainWindow();
                    device.Process.Kill();
                }
            }

        }

        private static string GetFallbackPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (RuntimeInformation.OSArchitecture == Architecture.X64)
                    return "scrcpy-win-x64";
            }
            return string.Empty;
        }

        private static void ProcessCommand(string command)
        {
            switch (command)
            {
                case Commands.CMD_Help:
                    Commands.RunShowHelp();
                    break;
                case Commands.CMD_Get:
                    Commands.RunGetAvailableDevices();
                    break;
                case Commands.CMD_List:
                    Commands.RunListAvailableDevices();
                    break;
                case Commands.CMD_Select:
                    Commands.RunSelectDevice();
                    break;
                case Commands.CMD_Connect:
                    Commands.RunConnectToDevice();
                    break;
                case Commands.CMD_Clear:
                    Commands.RunClear();
                    break;
            }
        }

    }

}