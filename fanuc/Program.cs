using fanuc;
using System.Reflection;

namespace FanucRobotServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //TCPServer server = new(5000);
            //server.Start();
            //Console.ForegroundColor = ConsoleColor.Green;
            //Console.WriteLine("CTRL + C to stop the server...");
            //Console.ResetColor();
            //Console.ReadLine();
            //server.Stop();

            var kernelSettings = KernelSettings.LoadConfigurations();
            Console.WriteLine(kernelSettings.ToString());
        }
    }
}