using OpenAI_API;
using Newtonsoft.Json;
using System.Text;
using Aspose.Cells;

class GPTTesting
{
    static string ReadFromTxtFile(string path)
    {
        if (!File.Exists(path)) { throw new FileNotFoundException(path); }

        return File.ReadAllText(path);
    }

    static string FillPromptTemplate(string tmp, string cmd)
    {
        string prompt = string.Empty;
        string stay_cmd = "Please stay in the same location.";

        if (string.IsNullOrEmpty(tmp)) { throw new System.ArgumentNullException(nameof(tmp)); }

        if (!string.IsNullOrEmpty(cmd))
        {
            prompt = tmp + cmd;
        }
        else
        {
            prompt = tmp + stay_cmd;
        }

        return prompt;
    }

    static async Task<string> GetResponseFromGPT(string key, string prompt)
    {
        var openai = new OpenAIAPI(new APIAuthentication(key));

        var conversation = openai.Chat.CreateConversation();

        if (string.IsNullOrEmpty (prompt)) { throw new System.ArgumentNullException (nameof(prompt)); }

        conversation.AppendUserInput(prompt);
        string response = await conversation.GetResponseFromChatbotAsync();

        return response;
    }

    static void SaveResult(string path, string response)
    {
        if (string.IsNullOrEmpty(path)) { throw new System.ArgumentNullException(nameof(path)); }

        /* Some string formatting */

        File.WriteAllText(path, response);
    }

    //static async Task Main(string[] args)
    //{
    //    string key_json_path = "C:\\Users\\OpEx-Dev\\Documents\\Haoxuan_workspace\\C#\\chatbot\\OpenAI\\OpenAITesting\\OpenAITesting\\txt\\openai_api_key.txt";
    //    var openai_api_key = ReadFromTxtFile(key_json_path);
    //    //Console.WriteLine(openai_api_key);

    //    string prompt_template_path = "C:\\Users\\OpEx-Dev\\Documents\\Haoxuan_workspace\\C#\\chatbot\\OpenAI\\OpenAITesting\\OpenAITesting\\txt\\prompt_template.txt";
    //    var prompt_template = ReadFromTxtFile(prompt_template_path);
    //    //Console.WriteLine(prompt_template.ToString());

    //    Console.WriteLine("Please give a command to draw a trajectory ");
    //    var command = Console.ReadLine();
    //    string prompt = FillPromptTemplate(prompt_template, command);
    //    Console.WriteLine(prompt);

    //    Console.WriteLine("The prompt is sent to GPT, waiting response... ");
    //    string response = await GetResponseFromGPT(openai_api_key, prompt);
    //    Console.WriteLine(response);

    //    string temp_result_file = "C:\\Users\\OpEx-Dev\\Documents\\Haoxuan_workspace\\C#\\chatbot\\OpenAI\\OpenAITesting\\OpenAITesting\\txt\\temp_result.txt";
    //    Console.WriteLine("Saving the result to the file...");
    //    File.WriteAllText(temp_result_file, response);
    //    Console.WriteLine("The result is saved into a given file.");

    //    string result_json_path = "C:\\Users\\OpEx-Dev\\Documents\\Haoxuan_workspace\\C#\\chatbot\\OpenAI\\OpenAITesting\\OpenAITesting\\json\\trajectory.json";
    //    Console.WriteLine("Saving the result to the file...");
    //    SaveResult(result_json_path, response);
    //    Console.WriteLine("The result is converted into .json file.");
    //}
}
