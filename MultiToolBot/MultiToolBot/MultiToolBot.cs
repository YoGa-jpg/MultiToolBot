using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;
using MultiToolBot.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MultiToolBot
{
    class MultiToolBot
    {
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }

        //public MultiToolBot()
        //{
        //    var config = new DiscordConfiguration
        //    {
        //        Token = "ODIzNjYyMTM3NTYwOTI0MTcw.YFkFJA.7UlGh5Axb94mlPrLlN06OK_7B8I",
        //        TokenType = TokenType.Bot,
        //        AutoReconnect = true,
        //        Intents = DiscordIntents.All,
        //        MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug
        //    };

        //    Client = new DiscordClient(config);

        //    Client.Ready += (x, y) => Task.CompletedTask;


        //    var commandsConfig = new CommandsNextConfiguration
        //    {
        //        StringPrefixes = new string[] { "?", "Великий Си" },
        //        EnableDms = true,
        //        EnableMentionPrefix = true,
        //        DmHelp = true
        //    };

        //    Commands = Client.UseCommandsNext(commandsConfig);

        //    Commands.RegisterCommands<VoiceCommands>();

        //    Client.ConnectAsync();

        //    Task.Delay(-1);
        //}

        public async Task RunAsync()
        {
            var config = new DiscordConfiguration
            {
                Token = ,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                Intents = DiscordIntents.All,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug
            };

            Client = new DiscordClient(config);
            Client.UseVoiceNext();

            //Client.Ready += OnClientReady;

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { "?", "Великий Си" },
                EnableDms = true,
                EnableMentionPrefix = true,
                DmHelp = true
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.RegisterCommands<VoiceCommands>();

            await Client.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
