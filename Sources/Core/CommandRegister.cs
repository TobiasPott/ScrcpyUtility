using System;
using System.Collections.Generic;
using System.Linq;

namespace NoXP.Scrcpy
{
    public class CommandRegister
    {
        private static Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>();

        public static void Init()
        {
            Type t = typeof(ICommand);
            IEnumerable<Type> derivedTypes = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                                              from assemblyType in domainAssembly.GetTypes()
                                              where t.IsAssignableFrom(assemblyType) && !assemblyType.IsAbstract && !assemblyType.IsInterface
                                              select assemblyType);

            //register each available command
            foreach (Type type in derivedTypes)
                Activator.CreateInstance(type);
        }

        public static void Register(ICommand command)
        {
            if (!_commands.ContainsKey(command.Instruction))
                _commands.Add(command.Instruction, command);
        }
        public static void Unregister(ICommand command)
        {
            if (_commands.ContainsKey(command.Instruction))
                _commands.Remove(command.Instruction);
        }

        public static bool Exists(string instruction)
        {
            return _commands.ContainsKey(instruction);
        }
        public static bool Execute(string instruction, string argument = "")
        {
            if (_commands.TryGetValue(instruction, out ICommand command))
            {
                command.Execute(argument);
                return true;
            }
            return false;
        }
        public static bool Execute(Type commandType, string argument = "")
        {
            ICommand command = GetCommandByType(commandType);
            if (command != null)
            {
                command.Execute(argument);
                return true;
            }
            return false;
        }
        private static ICommand GetCommandByType(Type commandType)
        {
            return _commands.Values.FirstOrDefault((x) => x.GetType().Equals(commandType));
        }
        public static IEnumerable<ICommand> GetCommands(CommandTypes typeMask = (CommandTypes)int.MaxValue)
        {
            return _commands.Values.Where((x) => typeMask.HasFlag(x.Type));
        }

    }

}
