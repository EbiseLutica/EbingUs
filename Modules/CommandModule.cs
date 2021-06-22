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
        public CommandModule RegisterCommand(string name, AsyncCommandHandler handler, string? help = null, bool isHostOnly = true, bool isLobbyOnly = true)
        {
            commands.Add(name.ToLowerInvariant(), new Command(handler, isHostOnly, isLobbyOnly, help));
            Logger.LogInformation("Add " + name + " command");
            return this;
        }

        public override void OnEnabled()
        {
            RegisterCommand("help", async (args, body, e) => {
                var query = commands.AsEnumerable();
                if (!e.ClientPlayer.IsHost)
                {
                    query = query.Where(c => !c.Value.IsHostOnly);
                }
                if (e.Game.GameState != GameStates.NotStarted)
                {
                    query = query.Where(c => !c.Value.IsLobbyOnly);
                }
                return string.Join('\n', query.Select(c => $"{c.Key} - {c.Value.Help}"));
            });
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
            async ValueTask ChatAsync(string text)
            {
                var lines = text.Split('\n');
                foreach (var item in lines)
                {
                    await e.PlayerControl.SendChatToPlayerAsync(item);
                    await Task.Delay(100);
                }
            }
            var commanderName = e.ClientPlayer.Character?.PlayerInfo.PlayerName;

            e.IsCancelled = true;

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
        record Command(AsyncCommandHandler Handler, bool IsHostOnly, bool IsLobbyOnly, string? Help);
    }


    public delegate ValueTask<string> AsyncCommandHandler(string[] args, string body, IPlayerChatEvent e);
}