using System.Text.Json;

namespace fanuc
{
    internal class KernelSettings
    {
        private const string ConfigFile = "config.json";

        private const string TypeKey = "type";
        private const string ChatModelKey = "chatModel";
        private const string TextModelKey = "textModel";
        private const string EmbeddingModelKey = "embeddingModel";

        private const string TypeValue = "OpenAI";
        private const string ChatModelValue = "gpt-3.5-turbo";
        private const string TextModelValue = "text-davinci-003";
        private const string EmbeddingModelValue = "text-embedding-ada-002";

        private const string SecretKey = "apiKey";
        private const string OrgKey = "org";

        /// <summary>
        /// Load the kernel settings from config.json
        /// <summary>
        internal static (string type, string chat, string text, string embedding, string apiKey, string orgId)
            LoadConfigurations()
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), ConfigFile);

            // If the file not exist, then create a new config.json file
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Configuration file not found: {filePath}");
                Console.WriteLine("A new configuration file will be created.");
                CreateConfigFile(filePath);
                return ReadConfigFile(filePath);
            }

            return ReadConfigFile(filePath);
        }

        /// <summary>
        /// Create the config.json file if it not exist
        /// API key will be asked and obtained from console input
        /// </summary>
        /// <param name="filePath"></param>
        internal static void CreateConfigFile(string filePath)
        {
            Console.WriteLine("Creating the configuration file...");

            // API key is enquired
            Console.Write("Please provide your OpenAI API Key: ");
            var key = "";
            while (string.IsNullOrWhiteSpace(key))
            {
                key = Console.ReadLine();
            }

            try
            {
                // Structure the string into json format
                var data = new Dictionary<string, string>
                {
                    { TypeKey, TypeValue },
                    { ChatModelKey, ChatModelValue },
                    { TextModelKey, TextModelValue },
                    { EmbeddingModelKey, EmbeddingModelValue },
                    { SecretKey, key },
                    { OrgKey, string.Empty },
                };

                // Write the json file to the given path
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(filePath, JsonSerializer.Serialize(data, options));
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong: " + e.Message);
            }
        }

        /// <summary>
        /// Read configuration in the config.json
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        internal static (string type, string chat, string text, string embedding, string apiKey, string orgId)
            ReadConfigFile(string filePath)
        {
            string apikey = "";
            string orgId = "";

            try
            {
                Console.WriteLine("Configuration file is found for loading.");

                var config = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(filePath));
                if (config != null)
                {
                    apikey = config[SecretKey];
                    orgId = config[OrgKey];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong: " + ex.Message);
            }

            return (TypeValue, ChatModelValue, TextModelValue, EmbeddingModelValue, apikey, orgId);
        }
    }
}