using fanuc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

            // Load the kernel setting
            (string type, string chat, string text, string embedding, string apiKey, string orgId) = KernelSettings.LoadConfigurations();
            Console.WriteLine("Using service: " + type);
            Console.WriteLine("Using chat model: " + chat);
            Console.WriteLine("Using text model: " + text);
            Console.WriteLine("Using embedding model: " + embedding);
            Console.WriteLine("API Key: " + apiKey.Substring(0, 3) + "...");
        }
    }
}