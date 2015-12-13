namespace Synacor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;

    public class Binary : IDisposable
    {
        private readonly BinaryReader reader;

        public Binary(string fileName)
        {
            this.reader = new BinaryReader(File.OpenRead(fileName));
        }

        public void CopyTo(ushort[] arr)
        {
            foreach (var it in this.ReadValues()
                .Select((x, i) => new { Value = x, Index = i }))
            {
                arr[it.Index] = it.Value;
            }
        }

        public void Dispose()
        {
            reader.Dispose();
        }

        private IEnumerable<ushort> ReadValues()
        {
            while (this.reader.BaseStream.Length != this.reader.BaseStream.Position)
            {
                yield return this.reader.ReadUInt16();
            }

            this.reader.BaseStream.Seek(0, SeekOrigin.Begin);
        }
    }
}
