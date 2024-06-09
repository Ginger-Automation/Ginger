using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerCoreNET.GenAIServices
{
    public class ChatBotResponseInfo
    {
     public string IsError { get; set; }   
     public string message { get; set; }
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public string ExpiresIn { get; set; }

    }
}
