using PowerArgs;

namespace Binder.CLI
{
    internal class Program
    {
        private static void Main(string[] args)
        {

            Args.InvokeAction<CommandLineArguments>(args);
        }

    }

}