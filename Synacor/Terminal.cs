namespace Synacor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class Terminal
    {
        private readonly StringBuilder outputBuffer = new StringBuilder();

        private Queue<char> inputBuffer = new Queue<char>();

        public void WriteAscii(ushort num)
        {
            var character = (char)num;
            this.outputBuffer.Append(character);

            if (character == '\n')
            {
                Console.Write(this.outputBuffer.ToString());
                this.outputBuffer.Clear();
            }
        }

        public ushort ReadAscii()
        {
            if (!this.inputBuffer.Any())
            {
                foreach (var character in Console.ReadLine())
                {
                    this.inputBuffer.Enqueue(character);
                }

                this.inputBuffer.Enqueue('\n');
            }

            return (ushort)this.inputBuffer.Dequeue();
        }
    }
}
