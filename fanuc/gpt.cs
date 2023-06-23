using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenAI_API;

namespace FanucRobotServer
{
    public class GPT
    {
        public GPT() { }

        /// <summary>
        /// Get prompts from Unity string inputs interface
        /// </summary>
        /// <param name="server"></param>
        /// <returns>
        /// A prompt string 
        /// </returns>
        public string GetPromptFromUnity(TCPServer server)
        {
            return server.unity_cmd;
        }

        /// <summary>
        /// Load the OpenAI API key token from a json file 
        /// </summary>
        /// <param name="key_path"></param>
        /// The path of the file containing the key
        /// <returns></returns>
        /// The key in string
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public string LoadOpenAIKey(string key_path)
        {
            if (string.IsNullOrEmpty(key_path)) { throw new ArgumentNullException(); }
            if (!File.Exists(key_path)) { throw new FileNotFoundException(); }

            string json_text = File.ReadAllText(key_path);
            dynamic? json_obj = JsonConvert.DeserializeObject(json_text);

            string key = string.Empty;
            if (json_obj != null)
            {
                key = json_obj["key"];
                if (!string.IsNullOrEmpty(key))
                {
                    Console.WriteLine("Key loaded successfully: " + key.Substring(0, 5) + "..."); // prints the first 5 characters
                }
                else
                {
                    Console.WriteLine("No key is provided in the file.");
                }
            }
            else
            {
                Console.WriteLine("No key is provided.");
            }

            return key;
        }


        /// <summary>
        /// Read a configurated prompt template from a text file
        /// </summary>
        /// <param name="path"></param>
        /// The string of the path containing the prompt template
        /// <returns></returns>
        /// The prompt template in string
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public string LoadPromptTemplate(string path)
        {
            if (string.IsNullOrEmpty(path)) {  throw new ArgumentNullException(); }
            if (!File.Exists(path)) { throw new FileNotFoundException(path); }

            return File.ReadAllText(path);
        }

        /// <summary>
        /// Construct the prompt 
        /// </summary>
        /// Read the user command input and the prompt template
        /// Concatenate both into a single prompt for GPT generation
        /// <param name="promp_tmp"></param>
        /// The prompt template read from a text file
        /// <param name="cmd"></param>
        /// The command read from user input
        /// <returns></returns>
        /// A prompt in single string
        /// <exception cref="ArgumentNullException"></exception>
        public string ConstructPrompt(string promp_tmp, string cmd)
        {
            if (string.IsNullOrEmpty(promp_tmp)) {
                Console.WriteLine("Please set up the prompt template.");
                throw new ArgumentNullException(nameof(promp_tmp));
            }
            
            string prompt;
            string stay_cmd = "Please stay in the same location.";
            if (!string.IsNullOrEmpty(cmd)) { prompt = promp_tmp + cmd; }
            else {prompt = promp_tmp + stay_cmd; }

            return prompt;
        }

        /// <summary>
        /// The core function to call OpenAI GPT API
        /// </summary>
        /// Connect to the OpenAI GPT to create a conversation 
        /// <param name="key"></param>
        /// <param name="prompt"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<string> GetResponseFromGPT(string key, string prompt)
        {
            if (string.IsNullOrEmpty(prompt)) { throw new ArgumentNullException(nameof(prompt)); }

            try
            {
                var openai = new OpenAIAPI(new APIAuthentication(key));
                var conversation = openai.Chat.CreateConversation();
                conversation.AppendUserInput(prompt);
                //Console.WriteLine(prompt);
                string response = await conversation.GetResponseFromChatbotAsync();

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error encountered: {ex.Message}");
                throw;
            }
        }



        /// <summary>
        /// Save and update the generated path into a given json file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="response"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void SaveResult(string path, string response)
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
                Console.WriteLine("Invalid JSON response: " + response);
            }
        }

    }
}
