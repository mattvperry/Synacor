namespace Synacor
{
    using System;
    using System.Collections.Generic;

    public class VirtualMachine
    {
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
            this.memory = new ushort[32768];
            this.registers = new ushort[8];
            this.stack = new Stack<ushort>();
            this.term = new Terminal();
            this.instructionSet = new Dictionary<OpCodes, Action>
            {
                [OpCodes.Halt]  = () => this.halted = true,
                [OpCodes.Set]   = this.ExecuteOp(this.Set),
                [OpCodes.Push]  = this.ExecuteOp(this.Push),
                [OpCodes.Pop]   = this.ExecuteOp(this.Pop),
                [OpCodes.Eq]    = this.ExecuteOp(this.Eq),
                [OpCodes.Gt]    = this.ExecuteOp(this.Gt),
                [OpCodes.Jmp]   = this.ExecuteOp(this.Jmp),
                [OpCodes.Jt]    = this.ExecuteOp(this.Jt),
                [OpCodes.Jf]    = this.ExecuteOp(this.Jf),
                [OpCodes.Add]   = this.ExecuteOp(this.Add),
                [OpCodes.Mult]  = this.ExecuteOp(this.Mult),
                [OpCodes.Mod]   = this.ExecuteOp(this.Mod),
                [OpCodes.And]   = this.ExecuteOp(this.And),
                [OpCodes.Or]    = this.ExecuteOp(this.Or),
                [OpCodes.Not]   = this.ExecuteOp(this.Not),
                [OpCodes.Rmem]  = this.ExecuteOp(this.Rmem),
                [OpCodes.Wmem]  = this.ExecuteOp(this.Wmem),
                [OpCodes.Call]  = this.ExecuteOp(this.Call),
                [OpCodes.Ret]   = () => this.instructionPointer = this.stack.Pop(),
                [OpCodes.Out]   = this.ExecuteOp(this.Out),
                [OpCodes.In]    = this.ExecuteOp(this.In),
                [OpCodes.Noop]  = () => { },
            };
        }

        public void ExecuteBinary(string fileName)
        {
            this.ReadBinaryIntoMemory(fileName);
            this.Execute();
        }
        
        private Action ExecuteOp(Action<ushort> action) => 
            () => action(this.ReadAndAdvance());

        private Action ExecuteOp(Action<ushort, ushort> action) => 
            () => action(this.ReadAndAdvance(), this.ReadAndAdvance());

        private Action ExecuteOp(Action<ushort, ushort, ushort> action) => 
            () => action(this.ReadAndAdvance(), this.ReadAndAdvance(), this.ReadAndAdvance());

        private void Set(ushort a, ushort b) => this.SetRegister(a, this.GetValue(b));

        private void Push(ushort a) => this.stack.Push(this.GetValue(a));

        private void Pop(ushort a) => this.SetRegister(a, stack.Pop());

        private void Eq(ushort a, ushort b, ushort c) => this.SetRegister(a, this.GetValue(b) == this.GetValue(c) ? (ushort)1 : (ushort)0);

        private void Gt(ushort a, ushort b, ushort c) => this.SetRegister(a, this.GetValue(b) > this.GetValue(c) ? (ushort)1 : (ushort)0);

        private void Jmp(ushort a) => this.instructionPointer = this.GetValue(a);

        private void Jt(ushort a, ushort b)
        {
            if (this.GetValue(a) > 0)
            {
                this.instructionPointer = this.GetValue(b);
            }
        }

        private void Jf(ushort a, ushort b)
        {
            if (this.GetValue(a) == 0)
            {
                this.instructionPointer = this.GetValue(b);
            }
        }

        private void Add(ushort a, ushort b, ushort c) => this.SetRegister(a, (ushort)((this.GetValue(b) + this.GetValue(c)) % 32768));

        private void Mult(ushort a, ushort b, ushort c) => this.SetRegister(a, (ushort)((this.GetValue(b) * this.GetValue(c)) % 32768));

        private void Mod(ushort a, ushort b, ushort c) => this.SetRegister(a, (ushort)((this.GetValue(b) % this.GetValue(c)) % 32768));

        private void And(ushort a, ushort b, ushort c) => this.SetRegister(a, (ushort)(this.GetValue(b) & this.GetValue(c)));

        private void Or(ushort a, ushort b, ushort c) => this.SetRegister(a, (ushort)(this.GetValue(b) | this.GetValue(c)));

        private void Not(ushort a, ushort b) => this.SetRegister(a, (ushort)(~this.GetValue(b) & 0x7FFF));

        private void Rmem(ushort a, ushort b) => this.SetRegister(a, this.memory[this.GetValue(b)]);

        private void Wmem(ushort a, ushort b) => this.memory[this.GetValue(a)] = this.GetValue(b);

        private void Call(ushort a)
        {
            this.stack.Push(this.instructionPointer);
            this.instructionPointer = this.GetValue(a);
        }

        private void Ret() => this.instructionPointer = this.stack.Pop();

        private void Out(ushort a) => this.term.WriteAscii(this.GetValue(a));

        private void In(ushort a) => this.SetRegister(a, this.term.ReadAscii());

        private void SetRegister(ushort param, ushort value)
        {
            this.registers[param - 32768] = value;
        }

        private ushort GetValue(ushort param)
        {
            return (ushort)(param < 32768 ? param : this.registers[param - 32768]);
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
