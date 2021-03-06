#region using
using System;
using System.Reflection;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
#endregion

namespace TojaBot
{
    class Program
    {
        private DiscordSocketClient Client;
        private CommandService Commands;

        static void Main(string[] args)
        => new Program().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync()
        {
            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug
            });

            Commands = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = true,
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Debug
            });

            Client.MessageReceived += Client_MessageReceived;
            await Commands.AddModulesAsync(Assembly.GetEntryAssembly());

            Client.Ready += Client_Ready;
            Client.Log += Client_Log;
            Client.UserJoined += Client_UserJoined;
            Client.UserLeft += Client_UserLeft;

            string Token = "1234"; //Edit This <Bot Token ID>
            await Client.LoginAsync(TokenType.Bot, Token);
            await Client.StartAsync();

            await Task.Delay(-1);

        }

        private async Task Client_UserLeft(SocketGuildUser user)
        {
            var channel = Client.GetChannel(1234) as SocketTextChannel; //Edit this! <Channel ID>

            EmbedBuilder Embed = new EmbedBuilder();
            Embed.WithAuthor("Member Left");
            Embed.WithColor(0, 255, 0);
            Embed.WithDescription($"Rest In Pepperonis {user.Mention}");

            await channel.SendMessageAsync("", false, Embed.Build());
        }

        private async Task Client_UserJoined(SocketGuildUser user)
        {
            var channel = Client.GetChannel(1234) as SocketTextChannel; //Edit this! <Channel ID>

            EmbedBuilder Embed = new EmbedBuilder();
            Embed.WithAuthor("Member Joined");
            Embed.WithColor(0, 0, 255);
            Embed.WithDescription($"Dobrodosao {user.Mention} kod {channel.Guild.Name}");

            await channel.SendMessageAsync("", false, Embed.Build());

            //Assign role
            ulong roleID = 1234; //Edit This! <Member role ID>
            var role = user.Guild.GetRole(roleID);
            await user.AddRoleAsync(role);
        }

        private async Task Client_Log(LogMessage Message)
        {
            Console.WriteLine($"{DateTime.Now} at {Message.Source}] {Message.Message}");
        }

        private async Task Client_Ready()
        {
            await Client.SetGameAsync("TojaBot - Running on " +
                "Server", "https://www.ostoja.tk/", StreamType.NotStreaming);
        }

        private async Task Client_MessageReceived(SocketMessage MessageParam)
        {
            var Message = MessageParam as SocketUserMessage;
            var Context = new SocketCommandContext(Client, Message);

            if (Context.Message == null || Context.Message.Content == "") return;
            if (Context.User.IsBot) return;

            int ArgPos = 0;
            if (!(Message.HasStringPrefix("!", ref ArgPos) || Message.HasMentionPrefix(Client.CurrentUser, ref ArgPos))) return;

            var Result = await Commands.ExecuteAsync(Context, ArgPos);
            if (!Result.IsSuccess)
            {
                Console.WriteLine($"{DateTime.Now} at Commands] Something went wrong with executing a command. Text: {Context.Message.Content} | Error: {Result.ErrorReason}");

                EmbedBuilder Embed = new EmbedBuilder();
                Embed.WithAuthor("Command Error", Context.User.GetAvatarUrl());
                Embed.WithColor(255, 0, 0);
                Embed.WithDescription($"Text: {Context.Message.Content}\n" + $"Error: {Result.ErrorReason}");

                await Context.Channel.SendMessageAsync("", false, Embed.Build());
            }
        }
    }
}
