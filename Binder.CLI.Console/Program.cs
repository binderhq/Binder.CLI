using PowerArgs;
using System;

namespace Binder.CLI
{
    internal class Program
    {
        private static void Main(string[] args)
        {
			Args.InvokeAction<CommandLineArguments>(args);
			Console.WriteLine("Upload complete. Press any key to exit.");
			Console.ReadKey();
        }

    }

}