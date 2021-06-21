using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Games;
using Impostor.Api.Innersloth;
using Impostor.Api.Innersloth.Customization;
using Impostor.Api.Net;
using Microsoft.Extensions.Logging;

namespace EbingUs
{
    /// <summary>
    /// コマンドハンドル用モジュール
    /// </summary>
    public class CommandModule : ModuleBase<CommandModule>
    {
        public void RegisterCommand(string name, AsyncCommandHandler handler, bool isHostOnly = true, bool isLobbyOnly = true)
        {
            commands.Add(name.ToLowerInvariant(), new Command(handler, isHostOnly, isLobbyOnly));
            Logger.LogInformation("Add " + name + " command");
        }

        public override void OnDisabled()
        {
            commands.Clear();
        }

        [EventListener]
        public void OnPlayerChat(IPlayerChatEvent e)
        {
            if (!e.Message.StartsWith("/"))
            {
                Logger.LogInformation($"Not a command");
                return;
            }
            Logger.LogInformation($"Run Command");
            Task.Run(async () => await DoCommands(e));
        }

        private async ValueTask DoCommands(IPlayerChatEvent e)
        {
            var parts = e.Message[1..].Split(" ");
            var name = parts[0].ToLowerInvariant();
            var args = parts[1..];
            var body = string.Join(" ", args);
            ValueTask ChatAsync(string text) => e.PlayerControl.SendChatAsync(text);

            var commanderName = e.ClientPlayer.Character?.PlayerInfo.PlayerName;

            if (!commands.ContainsKey(name))
            {
                await ChatAsync("そんなコマンドはないんよ");
                Logger.LogInformation($"Player {commanderName} tried to issue a command that does not exist.");
                return;
            }

            var command = commands[name];
            if (command.IsHostOnly && !e.ClientPlayer.IsHost)
            {
                await ChatAsync("それは村のえらい人だけが使えるコマンドなんよ");
                Logger.LogInformation($"Player {commanderName} tried to issue a host-only command.");
                return;
            }
            if (command.IsLobbyOnly && e.Game.GameState != GameStates.NotStarted)
            {
                await ChatAsync("ロビー以外では使えないんよ");
                Logger.LogInformation($"Player {commanderName} tried to issue a lobby-only command.");
                return;
            }

            var response = await command.Handler(args, body, e);
            if (!string.IsNullOrWhiteSpace(response))
            {
                await ChatAsync(response);
                Logger.LogInformation($"Player {commanderName} succeeded to issue the command.");
            }
        }

        private readonly Dictionary<string, Command> commands = new ();
    }

    public record Command(AsyncCommandHandler Handler, bool IsHostOnly, bool IsLobbyOnly);

    public delegate ValueTask<string> AsyncCommandHandler(string[] args, string body, IPlayerChatEvent e);
}