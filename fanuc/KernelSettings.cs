using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace fanuc
{
    internal class KernelSettings
    {
        private const string ConfigFile = "config.json";
        private const string TypeKey = "type";
        private const string ModelKey = "model";
        private const string SecretKey = "apiKey";
        private const string OrgKey = "org";
        private const string TypeValue = "OpenAI";
        private const string ModelValue = "gpt-3.5-turbo";

        /// <summary>
        /// Load the kernel settings from config.json
        /// <summary>
        internal static KernelSettings LoadConfigurations()
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), ConfigFile);

            // If the file not exist, then create a new config.json file
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Configuration file not found: {filePath}");
                Console.WriteLine("A new configuration file will be created.");
                CreateConfigFile(filePath);
            }

            // If exists, then read the file and load the config.json
            Console.WriteLine("Configuration file is found for loading.");

            return ReadConfigFile();
        }

        /// <summary>
        /// Create the config.json file if it not exist
        /// API key will be asked and obtained from console input
        /// </summary>
        /// <param name="filePath"></param>
        internal static KernelSettings CreateConfigFile(string filePath)
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
                // Organize the settings in json format 
                var configs = new Dictionary<string, string>
                {
                    { TypeKey, TypeValue },
                    { ModelKey, ModelValue },
                    { SecretKey, key },
                    { OrgKey, string.Empty },
                };

                // Write the formed json file to the defined path
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(filePath, JsonSerializer.Serialize(configs, options));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong: " + ex.Message);
            }
            
            return ReadConfigFile();
        }

        /// <summary>
        /// Read the configuration file
        /// </summary>
        internal static KernelSettings ReadConfigFile()
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), ConfigFile);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(filePath, optional: true, reloadOnChange: true)
                .Build();

            return configuration.Get<KernelSettings>()
                ?? throw new InvalidOperationException($"Invalid semantic kernel settings.");
        }
    }
}