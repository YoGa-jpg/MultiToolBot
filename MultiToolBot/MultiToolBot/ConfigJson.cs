using Newtonsoft.Json;

namespace MultiToolBot
{
    public class ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("prefix")]
        public string CommandPrefix { get; private set; }

        [JsonProperty("host")]
        public string Hostname { get; private set; }

        [JsonProperty("port")]
        public int Port { get; set; }
    }
}