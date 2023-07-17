using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.SkillDefinition;
using System.ComponentModel;

namespace fanuc.skills
{
    internal class ChatSkill
    {
        private readonly IChatCompletion _chatCompletion;
        private readonly ChatHistory _chatHistory;
        private Prompts prompts = new Prompts();

        // Define the role during the chatting
        private readonly Dictionary<AuthorRole, string> _roleDisplay = new()
        {
            {AuthorRole.System,    "System:    " },
            {AuthorRole.User,      "User:      " },
            {AuthorRole.Assistant, "Assistant: " }
        };

        // Define font color for each role's text shown on the console 
        private readonly Dictionary<AuthorRole, ConsoleColor> _roleConsoleColor = new()
        {
            {AuthorRole.System,    ConsoleColor.DarkCyan },
            {AuthorRole.User,      ConsoleColor.Yellow },
            {AuthorRole.Assistant, ConsoleColor.Green },
        };

        // For initialize the chat with system prompt
        public ChatSkill(IKernel kernel)
        {
            _chatCompletion = kernel.GetService<IChatCompletion>();
            // Add the system prompt to the chatGPT chatting history before the chat begin
            _chatHistory = _chatCompletion.CreateNewChat(prompts.SystemPrompt);
        }

        /// <summary>
        /// Send a prompt to the chat LLM
        /// 1. Add the user message to the chat history
        /// 2. The chat history, as a context for the prompt, will be sent to the chat LLM 
        /// 3. Get the response from the chatLLM with the given chat history
        /// </summary>
        /// <param name="prompt"></param>
        /// <returns></returns>
        [SKFunction, Description("Send a prompt to the LLM")]
        public async Task<string> PromptAsync(string prompt)
        {
            var reply = string.Empty;

            try
            {
                _chatHistory.AddMessage(AuthorRole.User, prompt);
                reply = await _chatCompletion.GenerateMessageAsync(_chatHistory);
            }
            catch (AIException aiex)
            {
                reply = $"OpenAI returned an error ({aiex.Message}). Please try again.";
            }

            return reply;
        }

        /// <summary>
        /// Log the history of the chat with the LLM
        /// Log the system prompt that configures the chat, with the user and assistant messages.
        /// </summary>
        /// <returns></returns>
        [SKFunction, Description("Log the history of the chat with the LLM")]
        public Task LogChatHistory()
        {
            Console.WriteLine();
            Console.WriteLine("==========================================");
            Console.WriteLine("Chat history: ");
            Console.WriteLine();

            foreach (var message in _chatHistory.Messages)
            {
                string role = "None:      ";

                // The role 
                if (_roleDisplay.TryGetValue(message.Role, out var displayRole)) 
                {
                    role = displayRole;
                }

                // The color
                if (_roleConsoleColor.TryGetValue(message.Role, out var color))
                {
                    Console.ForegroundColor = color;
                }

                // Write the role message with given color
                Console.WriteLine($"{role}{message.Content}");
            }

            return Task.CompletedTask;
        }
    }
}