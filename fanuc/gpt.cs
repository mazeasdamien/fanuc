using Newtonsoft.Json;
using OpenAI_API;
using OpenAI_API.Chat;

namespace FanucRobotServer
{
    public class GPT
    {
        /// <summary>
        /// The core function to call OpenAI GPT API
        /// </summary>
        /// Connect to the OpenAI GPT to create a conversation 
        /// <param name="key"></param>
        /// <param name="prompt"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<string> GetResponseFromGPT(string key, List<ChatMessage> chatHistory, string modelId)
        {
            try
            {
                var openai = new OpenAIAPI(new APIAuthentication(key));

                var request = new ChatRequest()
                {
                    Model = modelId,
                    Messages = chatHistory.ToArray()
                };

                var result = await openai.Chat.CreateChatCompletionAsync(request);
                var reply = result.Choices[0].Message.Content;

                // Add the reply to the chat history
                chatHistory.Add(new ChatMessage { Role = ChatMessageRole.Assistant, Content = reply });

                return $"{reply.Trim()}";
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error encountered: {ex.Message}");
                Console.ResetColor();
                throw;
            }
        }


        /// <summary>
        /// Save and update the generated path into a given json file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="response"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void SaveResult(string path, string response)
        {
            if (string.IsNullOrEmpty(path)) { throw new ArgumentNullException(nameof(path)); }

            // Find the first { and last } in the response
            int firstBrace = response.IndexOf('{');
            int lastBrace = response.LastIndexOf('}');

            // If both braces were found, extract the substring between them
            if (firstBrace >= 0 && lastBrace >= 0 && lastBrace > firstBrace)
            {
                string json = response.Substring(firstBrace, lastBrace - firstBrace + 1);

                // Save the extracted JSON
                File.WriteAllText(path, json);
            }
            else
            {
                // Handle the case where the response doesn't contain valid JSON
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid JSON response: " + response);
                Console.ResetColor();
            }
        }

    }
}
