using System;
using System.Collections.Generic;
using System.Text;

namespace NoXP.Scrcpy
{
    public enum CommandTypes
    {
        Global = 1,
        Device = 2
    }
    public interface ICommand
    {
        string Instruction { get; }

        CommandTypes Type { get; }
        string Help { get; }

        bool CheckInstruction(string instruction);
        void Execute(string args = "");
    }
    public abstract class CommandBase<T> : ICommand where T : CommandBase<T>, new()
    {
        protected static T Instance { get; } = new T();

        public virtual string Instruction
        { get; protected set; }

        public virtual CommandTypes Type
        { get; protected set; }

        public virtual string Help
        { get; protected set; }


        public CommandBase(string instruction, CommandTypes type, string help)
        {
            this.Instruction = instruction;
            this.Type = type;
            this.Help = help;
            CommandRegister.Register(this);
        }


        public bool CheckInstruction(string instruction)
        { return instruction.Equals(this.Instruction); }

        public abstract void Execute(string args = "");
    }

}

