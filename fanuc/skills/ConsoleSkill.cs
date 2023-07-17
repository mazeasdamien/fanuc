using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
using System.ComponentModel;


namespace fanuc.skills
{
    internal class ConsoleSkill
    {
        private bool _isGoodbye = false;

        /// <summary>
        /// Gets input from the console
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [SKFunction, Description("Get console input.")]
        public Task<string> ListenAsync(SKContext context) 
        {
            return Task.Run(() => 
            {
                var line = "";

                Console.Write("User: ");
                while (string.IsNullOrWhiteSpace(line))
                {
                    line = Console.ReadLine();
                }

                if (line.ToLower().Contains("goodbye"))
                {
                    _isGoodbye = true; 
                }

                return line;
            });
        }

        /// <summary>
        /// Write output to the console
        /// </summary>
        /// <param name="message"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [SKFunction, Description("Write a response to the console.")]
        public Task<string> RespondAsync(string message, SKContext context) 
        {
            return Task.Run(() => 
            {
                Console.Write("chatGPT: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(message);
                Console.ResetColor();

                return message;
            });
        }

        /// <summary>
        /// Checks if the user said goodbye
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [SKFunction, Description("If the user say goodbye.")]
        public Task<string> IsGoodbyeAsync(SKContext context) 
        {
            return Task.FromResult(this._isGoodbye ? "true" : "false");
        }
    }
}