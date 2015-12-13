namespace Synacor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var vm = new VirtualMachine();
            vm.ExecuteBinary("challenge/challenge.bin");
        }
    }
}
