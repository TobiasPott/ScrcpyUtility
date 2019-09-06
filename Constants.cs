using System;
using System.Runtime.InteropServices;

namespace NoXP.Scrcpy
{
    public class Constants
    {

        public static string ADB
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return "scrcpy.adb";
                else
                    return "adb";
            }
        }
        public const string ADB_TCPIP_PORT = "5555";

        public const string ADB_COMMAND_DEVICES = "devices";
        public const string ADB_COMMAND_TCPIP = "tcpip";
        public const string ADB_COMMAND_CONNECT = "connect";
        public const string ADB_COMMAND_DISCONNECT = "disconnect";
        public const string ADB_COMMAND_SHELL_IFCONFIG = "shell ifconfig";
        public const string ADB_COMMAND_SHELL_IPADDRSHOW = "shell ip addr show {0}";


        public static string SCRCPY
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return "scrcpy";
                else
                    return "scrcpy";
            }
        }

        public const string SCRCPY_ARG_SERIAL = "--serial {0} "; // use with string.Format to fill in devices serial
        public const string SCRCPY_ARG_MAXSIE = "--max-size {0} "; // use with string.Format to fill in maximum size (for largest dimension)
        public const string SCRCPY_ARG_BITRATE = "--bit-rate {0}M "; // use with string.Format to fill video bitrate (in Mb/s, default is 8 Mb/s)
        public const string SCRCPY_ARG_CROP = "--crop {0}:{1}:{2}:{3} "; // use with string.Format to fill in cropping area (width, height, x-offset, y-offset)
        public const string SCRCPY_ARG_NOCONTROL = "--no-control "; // use this to disable inputs from the scrcpy


        public static readonly string[] SeparatorNewLine = new string[] { Environment.NewLine };
        public static readonly char[] SeparatorWhitespace = new char[] { ' ', '\t' };
    }

}
