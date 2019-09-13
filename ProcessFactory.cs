using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace NoXP.Scrcpy
{
    public class ProcessFactory
    {
        public static string BasePath { get; set; } = string.Empty;



        public static Process CreateProcessADB(string arguments, bool redirect = true)
        {

            if (!string.IsNullOrEmpty(Constants.ADB))
            {
                Process proc = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo(Path.Combine(BasePath, Constants.ADB), arguments);
                startInfo.WorkingDirectory = BasePath;
                startInfo.RedirectStandardOutput = redirect;
                // disables creation of a window and thus printing stuff to the main console
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;

                proc.StartInfo = startInfo;
                return proc;
            }
            return null;
        }
        public static Process CreateProcessScrcpy(string arguments)
        {
            if (!string.IsNullOrEmpty(Constants.SCRCPY))
            {
                Process proc = new Process();
                string filename = Path.Combine(BasePath, Constants.SCRCPY);
                ProcessStartInfo startInfo = new ProcessStartInfo(filename, arguments);

                // disables creation of a window and thus printing stuff to the main console
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    startInfo.FileName = Constants.SCRCPY;
                }

                proc.StartInfo = startInfo;
                return proc;
            }
            return null;
        }


    }

}
