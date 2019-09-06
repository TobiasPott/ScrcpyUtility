using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NoXP.Scrcpy
{



    internal class Program
    {
        private static string _basePath = string.Empty;
        private static List<ADBDevice> _devices = new List<ADBDevice>();
        private static ADBDevice _currentDevice = null;
        private static int _currentDeviceIndex = -1;

        private static string ADBPath { get => Constants.ADB; } // Path.Combine(_basePath, Constants.ADB); }
        private static string ScrcpyPath { get => Constants.SCRCPY; } //Path.Combine(_basePath, Constants.SCRCPY); }

        private static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                _basePath = args[0];

                Console.WriteLine("Path to scrcpy: " + _basePath);
                Program.GetDevices();
                Console.WriteLine("Connected devices:");
                if (_devices.Count > 0)
                {
                    for (int i = 0; i < _devices.Count; i++)
                    {
                        Program.GetDevicesIpAddress(_devices[0]);
                        Console.WriteLine(string.Format("[{0,3}] {1}: [{2}]", i, _devices[i].ToString(), _devices[i].IpAddress));
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("No devices connected. Connect an android device with enabled debugging and restart this application.");
                    Console.ResetColor();
                    return;
                }
                // select device by index (this will be put into a loop later on)
                ADBDevice currentDevice = Program.SelectDevice();
                if (currentDevice != null)
                    Console.WriteLine(string.Format("Selected: {0}", currentDevice));
                else
                    Console.WriteLine("No device selected.");


                //Console.WriteLine();
                //if (Program.SetTCPIP())
                //    Console.WriteLine("Started in TCP/IP mode.");
                //else
                //    Console.WriteLine("An error occured when restarting in TPC/IP mode.");

                //Console.WriteLine();
                //if (Program.ConnectDevice(currentDevice))
                //    Console.WriteLine("Connected to selected device.");
                //else
                //    Console.WriteLine("An error occured when connecting to selected device.");

                if (currentDevice != null)
                {
                    ScrcpyArguments arguments = new ScrcpyArguments(false, -1, -1, -1, -1, 1280, -1, currentDevice.Serial);
                    StartScrcpy(arguments);
                }


                Console.WriteLine();
                Console.Write("Press key to end...");
                Console.ReadKey();

            }
        }

        private static void ApplicationLoop()
        {
            string input = "";
            do
            {
                Console.WriteLine("Enter 'quit' to quit the application.");
                Console.Write("|>> ");
                input = Console.ReadLine();

            }
            while (true);
        }

        private static void ProcessCommand(string command)
        {
            
        }





        private static Process CreateProcessADB(string arguments, bool redirect = true)
        {

            if (!string.IsNullOrEmpty(Program.ADBPath))
            {
                Process proc = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo(Path.Combine(_basePath, Program.ADBPath), arguments);
                startInfo.WorkingDirectory = _basePath;
                startInfo.RedirectStandardOutput = redirect;
                proc.StartInfo = startInfo;
                return proc;
            }
            return null;
        }
        private static Process CreateProcessScrcpy(string arguments)
        {
            if (!string.IsNullOrEmpty(Program.ScrcpyPath))
            {
                Process proc = new Process();
                string filename = Path.Combine(_basePath, Program.ScrcpyPath);
                ProcessStartInfo startInfo = new ProcessStartInfo(filename, arguments);
                startInfo.UseShellExecute = true;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    startInfo.UseShellExecute = false;
                    startInfo.FileName = Program.ScrcpyPath;
                }

                proc.StartInfo = startInfo;
                return proc;
            }
            return null;
        }


        private static void GetDevices()
        {
            if (!string.IsNullOrEmpty(Program.ADBPath))
            {
                Process proc = CreateProcessADB(Constants.ADB_COMMAND_DEVICES);
                proc.Start();

                string output = proc.StandardOutput.ReadToEnd();
                string[] lines = output.Split(Constants.SeparatorNewLine, StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length > 1)
                {
                    _devices.Clear();
                    for (int i = 1; i < lines.Length; i++)
                    {
                        string[] deviceInfo = lines[i].Split(Constants.SeparatorWhitespace, 2);
                        _devices.Add(new ADBDevice(deviceInfo[0], deviceInfo[1]));
                    }
                }
            }

        }
        private static void GetDevicesIpAddress(ADBDevice device)
        {
            if (!string.IsNullOrEmpty(Program.ADBPath))
            {
                if (!GetDeviceIpAddressFromIfconfig(device))
                {
                    // do try to retrieve the ipv4 address via "ip addr show" command
                    GetDeviceIpAddressFromIpAddrShow(device);
                }

            }

        }

        private static bool GetDeviceIpAddressFromIfconfig(ADBDevice device)
        {

            if (!string.IsNullOrEmpty(Program.ADBPath))
            {
                const string InetAddr = "inet addr:";
                const string Bcast = "Bcast:";
                const string Denied = "denied";
                const string Wlan = "wlan";

                string arguments = string.Empty;
                arguments += string.Format("-s {0} ", device.Serial);
                arguments += Constants.ADB_COMMAND_SHELL_IFCONFIG;

                Process proc = CreateProcessADB(arguments);
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

        // ! ! ! !
        // convert processing of output/input stream to use the event receiver method to hide it's output on the apps console
        // https://stackoverflow.com/questions/43668920/stop-process-output-from-being-displayed-in-project-command-window
        // ! ! ! !
        private static bool GetDeviceIpAddressFromIpAddrShow(ADBDevice device)
        {

            if (!string.IsNullOrEmpty(Program.ADBPath))
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

                    Process proc = CreateProcessADB(arguments);
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




        private static bool SetTCPIP()
        {
            if (!string.IsNullOrEmpty(Program.ADBPath))
            {
                const string Restarting = "restarting ";
                string arguments = Constants.ADB_COMMAND_TCPIP + " " + Constants.ADB_TCPIP_PORT;

                Process proc = CreateProcessADB(arguments);
                proc.Start();

                string output = proc.StandardOutput.ReadToEnd().ToLowerInvariant();
                if (output.StartsWith(Restarting))
                    return true;
            }
            return false;
        }

        private static ADBDevice SelectDevice()
        {
            Console.Write("Enter the index of the device to connect to [leave blank to use first device in list]:");
            string input = Console.ReadLine();
            if (string.IsNullOrEmpty(input))
            {
                if (_devices.Count > 0)
                    _currentDeviceIndex = 0;
                else
                    _currentDeviceIndex = -1; // invlaidates the index
            }
            else
            {
                int deviceIndex = -1;
                if (int.TryParse(input, out deviceIndex))
                    _currentDeviceIndex = deviceIndex;

                // invalidate index if out ouf device list range
                if (_currentDeviceIndex < 0 || _currentDeviceIndex >= _devices.Count)
                    _currentDeviceIndex = -1;
            }

            if (_currentDeviceIndex != -1)
                return _devices[_currentDeviceIndex];
            else
                return null;

        }

        private static bool ConnectDeviceOverWifi(ADBDevice device)
        {
            if (!string.IsNullOrEmpty(Program.ADBPath))
            {
                const string Connected = "connected ";

                string arguments = Constants.ADB_COMMAND_CONNECT + " " + device.IpAddress + ":" + Constants.ADB_TCPIP_PORT;

                Process proc = CreateProcessADB(arguments);
                proc.Start();

                string output = proc.StandardOutput.ReadToEnd().ToLowerInvariant();
                if (output.StartsWith(Connected))
                    return true;
            }
            return false;
        }

        private static async Task StartScrcpy(ScrcpyArguments arguments)
        {
            if (!string.IsNullOrEmpty(Program.ScrcpyPath))
            {
                Process proc = CreateProcessScrcpy(arguments.ToString());
                proc.Start();

                await Task.FromResult(0);
            }

        }

    }

}