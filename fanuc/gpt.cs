using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FanucRobotServer
{
    public class gpt
    {
        public string GetPromptFromUnity(TCPServer server)
        {
            /* Get prompts from unity interface */
            return server.unity_prompt;
        }
    }
}
