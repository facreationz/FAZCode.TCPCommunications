using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TestProject.Communications
{
    internal class HelperTools
    {
        /// <summary>
        /// Converts a message received from TCPCommunication into JObject, ensuring EndOfLineCharacters are unescaped.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="escapeCharacters"></param>
        /// <returns></returns>
        public static JObject MessageToJObject(string message, string escapeCharacters = "<!EOL!>")
        {
            // Escape characters. The quickest qay I've noted is that duplicate EscapeCharacters escept for its last character.
            string replaceCharacters = escapeCharacters.Substring(0, escapeCharacters.Length - 1);
            string json = message.Replace(replaceCharacters, escapeCharacters);

            // Return object
            return JObject.Parse(json);
        }


    }
}
