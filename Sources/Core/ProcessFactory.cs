using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace NoXP.Scrcpy
{
    public class ProcessFactory
    {
        public static string BasePath { get; private set; } = string.Empty;



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


        public static void SetBasePath(string basePath)
        {
            if (!string.IsNullOrEmpty(basePath))
            {
                ProcessFactory.BasePath = basePath;
            }
            else
                ProcessFactory.BasePath = GetFallbackPath();
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
    }

}
