using fanuc;
using fanuc.skills;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.SkillDefinition;

namespace FanucRobotServer
{
    class Program
    {
        static async Task Main(string[] args)
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

            var builder = new KernelBuilder();
            builder.WithOpenAIChatCompletionService(modelId: chat, apiKey: apiKey);
            
            IKernel kernel = builder.Build();

            var consoleSkill = kernel.ImportSkill(new ConsoleSkill());
            var chatSkill = kernel.ImportSkill(new ChatSkill(kernel));
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Connect to the OpenAI gpt-3.5-turbo successfully.");
            Console.ResetColor();

            await kernel.RunAsync("Hello. Ask me a question or say goodbye to exit.", consoleSkill["Respond"]);
        
            while (true)
            {
                ISKFunction[] pipeline = { consoleSkill["Listen"], chatSkill["Prompt"], consoleSkill["Respond"] };

                await kernel.RunAsync(pipeline);

                var goodbyeContext = await kernel.RunAsync(consoleSkill["IsGoodbye"]);
                var isGoodbye = bool.Parse(goodbyeContext.Result);

                if (isGoodbye)
                {
                    await kernel.RunAsync(chatSkill["LogChatHistory"]);
                    break;
                }
            }
        }
    }
}