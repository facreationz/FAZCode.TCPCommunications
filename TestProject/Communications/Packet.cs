using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TestProject.Communications
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Packet : IPacket
    {
        public string DataString { get; set; }
        public object DataObject { get; set; }

        public Packet() { }

        public Packet (int command, string message)
        {
            Command = command;
            DataString = message;
        }


        public string SerializeMessage(string escapeCharacters = "<!EOL!>")
        {
            // Serialize the message
            string json = JsonConvert.SerializeObject(this);

            // Escape characters. The quickest qay I've noted is that duplicate EscapeCharacters escept for its last character.
            string replaceCharacters = escapeCharacters.Substring(0, escapeCharacters.Length - 1);
            return json.Replace(escapeCharacters, replaceCharacters);
        }

        public static Packet DeserializeMessage(string message, string escapeCharacters = "<!EOL!>")
        {

            // Escape characters. The quickest qay I've noted is that duplicate EscapeCharacters escept for its last character.
            string replaceCharacters = escapeCharacters.Substring(0, escapeCharacters.Length - 1);
            string json = message.Replace(replaceCharacters, escapeCharacters);

            // Deserialize
            return JsonConvert.DeserializeObject<Packet>(json);
        }
    }
}
