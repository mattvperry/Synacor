namespace Synacor
{
    using System;
    using System.Collections.Generic;

    public class VirtualMachine
    {
        private const ushort ADDR_SPACE = 32768;

        private bool halted;

        private ushort instructionPointer;

        private ushort[] memory;

        private ushort[] registers;

        private Stack<ushort> stack;

        private Terminal term;

        private Dictionary<OpCodes, Action> instructionSet;

        public VirtualMachine()
        {
            this.halted = true;
            this.instructionPointer = 0;
            this.memory = new ushort[ADDR_SPACE];
            this.registers = new ushort[8];
            this.stack = new Stack<ushort>();
            this.term = new Terminal();
            this.instructionSet = new Dictionary<OpCodes, Action>
            {
                [OpCodes.Halt]  = () => this.halted = true,
                [OpCodes.Set]   = () => this.SetOp((b) => b),
                [OpCodes.Push]  = () => this.GetOp(this.stack.Push),
                [OpCodes.Pop]   = () => this.SetOp(this.stack.Pop),
                [OpCodes.Eq]    = () => this.SetOp((b, c) => b == c ? 1 : 0),
                [OpCodes.Gt]    = () => this.SetOp((b, c) => b > c ? 1 : 0),
                [OpCodes.Jmp]   = () => this.GetOp((a) => this.instructionPointer = a),
                [OpCodes.Jt]    = () => this.GetOp(this.ConditionalJump((a) => a > 0)),
                [OpCodes.Jf]    = () => this.GetOp(this.ConditionalJump((a) => a == 0)),
                [OpCodes.Add]   = () => this.SetOp((b, c) => (b + c) % ADDR_SPACE),
                [OpCodes.Mult]  = () => this.SetOp((b, c) => (b * c) % ADDR_SPACE),
                [OpCodes.Mod]   = () => this.SetOp((b, c) => (b % c) % ADDR_SPACE),
                [OpCodes.And]   = () => this.SetOp((b, c) => b & c),
                [OpCodes.Or]    = () => this.SetOp((b, c) => b | c),
                [OpCodes.Not]   = () => this.SetOp((b) => ~b & 0x7FFF),
                [OpCodes.Rmem]  = () => this.SetOp((b) => this.memory[b]),
                [OpCodes.Wmem]  = () => this.GetOp((a, b) => this.memory[a] = b),
                [OpCodes.Call]  = () => this.GetOp(this.Call),
                [OpCodes.Ret]   = () => this.instructionPointer = this.stack.Pop(),
                [OpCodes.Out]   = () => this.GetOp(this.term.WriteAscii),
                [OpCodes.In]    = () => this.SetOp(this.term.ReadAscii),
                [OpCodes.Noop]  = () => { },
            };
        }

        public void ExecuteBinary(string fileName)
        {
            this.ReadBinaryIntoMemory(fileName);
            this.Execute();
        }

        private Action<ushort, ushort> ConditionalJump(Func<ushort, bool> condition)
        {
            return (a, b) =>
            {
                if (condition(a))
                {
                    this.instructionPointer = b;
                }
            };
        }

        private void Call(ushort a)
        {
            this.stack.Push(this.instructionPointer);
            this.instructionPointer = a;
        }

        private void GetOp(Action<ushort> action)
        {
            var a = this.GetValue(this.ReadAndAdvance());
            action(a);
        }

        private void SetOp<T>(Func<T> action) where T : IConvertible
        {
            var a = this.ReadAndAdvance();
            this.SetRegister(a, action().ToUInt16(null));
        }

        private void GetOp(Action<ushort, ushort> action)
        {
            var a = this.GetValue(this.ReadAndAdvance());
            var b = this.GetValue(this.ReadAndAdvance());
            action(a, b);
        }

        private void SetOp<T>(Func<ushort, T> action) where T : IConvertible
        {
            var a = this.ReadAndAdvance();
            var b = this.GetValue(this.ReadAndAdvance());
            this.SetRegister(a, action(b).ToUInt16(null));
        }

        private void GetOp(Action<ushort, ushort, ushort> action)
        {
            var a = this.GetValue(this.ReadAndAdvance());
            var b = this.GetValue(this.ReadAndAdvance());
            var c = this.GetValue(this.ReadAndAdvance());
            action(a, b, c);
        }

        private void SetOp<T>(Func<ushort, ushort, T> action) where T : IConvertible
        {
            var a = this.ReadAndAdvance();
            var b = this.GetValue(this.ReadAndAdvance());
            var c = this.GetValue(this.ReadAndAdvance());
            this.SetRegister(a, action(b, c).ToUInt16(null));
        }

        private void SetRegister(ushort param, ushort value)
        {
            this.registers[param - ADDR_SPACE] = value;
        }

        private ushort GetValue(ushort param)
        {
            return (ushort)(param < ADDR_SPACE ? param : this.registers[param - ADDR_SPACE]);
        }

        private void Execute()
        {
            this.halted = false;
            while (!this.halted)
            {
                var opCode = (OpCodes)this.ReadAndAdvance();
                this.instructionSet[opCode]();
            }
        }

        private void ReadBinaryIntoMemory(string fileName)
        {
            using (var bin = new Binary(fileName))
            {
                bin.CopyTo(this.memory);
            }
        }

        private ushort ReadAndAdvance()
        {
            return this.memory[this.instructionPointer++];
        }
    }
}
