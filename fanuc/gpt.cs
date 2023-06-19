using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FanucRobotServer
{
    public class GPT
    {
        /// <summary>
        /// Get prompts from Unity string inputs interface
        /// </summary>
        /// <param name="server"></param>
        /// <returns>
        /// A prompt string 
        /// </returns>
        public string GetPromptFromUnity(TCPServer server)
        {
            return server.unity_prompt;
        }

        public void SendPromptToGPT(string prompt)
        { 

        }

        public void ReceivePromptFromGPT()
        {

        }

    }
}
