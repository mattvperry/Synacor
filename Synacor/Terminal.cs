namespace Synacor
{
    using System;

    public class Terminal
    {
        public void WriteAscii(ushort num)
        {
            Console.Write((char)num);
        }

        public ushort ReadAscii()
        {
            var keyInfo = Console.ReadKey();
            return keyInfo.Key == ConsoleKey.Enter ? (ushort)'\n' : (ushort)keyInfo.KeyChar;
        }
    }
}
