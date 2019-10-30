using System;

namespace NoXP.Scrcpy.CLI
{
    internal class Program
    {

        private static void Main(string[] args)
        {
            CommandRegister.Init();
            Console.BufferWidth = Math.Min(Console.LargestWindowWidth, 150);
            Console.WindowWidth = Math.Min(Console.LargestWindowWidth, 150);

            ProcessFactory.SetBasePath(args.Length > 0 ? args[0] : string.Empty);
            Console.WriteLine("Utility is using '" + ProcessFactory.BasePath + "' as the path to your scrcpy-installation.");

            // preset the scrcpy arguments with some default values
            ScrcpyArguments.Global.NoControl = false;
            ScrcpyArguments.Global.TurnScreenOff = true;
            ScrcpyArguments.Global.MaxSize = 1280;
            // prefills the list of devices available on this machine
            CommandRegister.Execute(typeof(Commands.List));
            //NoXP.Scrcpy.CLI.List.Exec();
            ADBDevice.SelectDevice("");

            string command = string.Empty;
            do
            {
                Console.WriteLine("Please enter your command (type 'quit' to terminate the application).");
                Console.Write("> ");

                command = Console.ReadLine();
                Program.ProcessCommand(command);
            }
            while (true);

        }

        private static void ProcessCommand(string command)
        {
            string arguments = "";
            if (command.Contains(' '))
            {
                string[] cmdArgs = command.Split(' ', 2);
                command = cmdArgs[0];
                arguments = cmdArgs[1];
            }

            CommandRegister.Execute(command, arguments);
        }

    }

}