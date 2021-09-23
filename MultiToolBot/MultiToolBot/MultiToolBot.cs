using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.VoiceNext;
using MultiToolBot.Commands;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using Newtonsoft.Json;

namespace MultiToolBot
{
    class MultiToolBot
    {
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }

        public VoiceNextExtension Voice { get; set; }
        public LavalinkExtension Lavalink { get; set; }

        public async Task RunAsync()
        {
            var json = "";
            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();
            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            var endpoint = new ConnectionEndpoint
            {
                Hostname = "127.0.0.1",
                Port = 2333
            };

            var botConfig = new DiscordConfiguration
            {
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                Intents = DiscordIntents.All,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug
            };
            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new[] { configJson.CommandPrefix },
                EnableDms = true,
                EnableMentionPrefix = true,
                DmHelp = true
            };
            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = "youshallnotpass",
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            Client = new DiscordClient(botConfig);
            Voice = Client.UseVoiceNext();
            Lavalink = Client.UseLavalink();
            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.RegisterCommands<VoiceCommands>();

            await Client.ConnectAsync();
            await Lavalink.ConnectAsync(lavalinkConfig);

            await Task.Delay(-1);
        }
    }
}
