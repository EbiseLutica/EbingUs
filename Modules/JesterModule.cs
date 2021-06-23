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
using Impostor.Api.Net.Inner;
using Impostor.Api.Net.Inner.Objects;
using Microsoft.Extensions.Logging;

namespace EbingUs
{
    /// <summary>
    /// てるてるモード
    /// </summary>
    public class JesterModule : ModuleBase<JesterModule>
    {
        /// <summary>
        /// てるてるモードを利用するゲームセッション
        /// </summary>
        internal class JesterSession
        {
            public IGame game;
            public IClientPlayer? jester;
            public bool jesterWon;

            public JesterSession(IGame game)
            {
                this.game = game;
            }
        }

        public override void OnEnabled()
        {
            // てるてる切り替えコマンド
            CommandModule.Instance.RegisterCommand("jester", async (args, body, e) => {
                switch (body.Trim().ToLowerInvariant())
                {
                    case "on":
                        if (sessions.ContainsKey(e.Game)) return "てるてるはもう有効";
                        sessions.Add(e.Game, new JesterSession(e.Game));
                        Logger.LogInformation($"Jester Mode is enabled!");
                        return "てるてるを有効化した";
                    case "off":
                        if (!sessions.ContainsKey(e.Game)) return "てるてるはもう無効";
                        sessions.Remove(e.Game);
                        Logger.LogInformation($"Jester Mode is disabled!");
                        return "てるてるを無効化した";
                    case "get":
                        return "てるてるは" + (sessions.ContainsKey(e.Game) ? "有効" : "無効");
                    default:
                        return "/jester <on|off>";
                }
            });
        }

        [EventListener]
        public async void OnGameStarted(IGameStartedEvent e)
        {
            var session = Get(e.Game);
            if (session == null) return;

            // クルーの中から選択
            var jester = e.Game.Players.Where(p => !p.Character!.PlayerInfo.IsImpostor).Random();

            if (jester == null)
            {
                await e.Game.Host!.Character!.SendChatAsync("てるてるはいません。(バグっす)");
                Logger.LogInformation($"For some reason, there is no jester");
                return;
            }

            // てるてるを割り当*てる*
            session.jester = jester;
            Logger.LogInformation($"Jester is {jester.Character?.PlayerInfo.PlayerName}.");

            // てるてるのタスクを終わらせる
            await Task.WhenAll(jester.Character!.PlayerInfo.Tasks.Select(c => c.CompleteAsync().AsTask()));

            await jester.Character!.SendChatToPlayerAsync("あなたが<color=#ff0000>てるてる</color>です。");
            await jester.Character!.SendChatToPlayerAsync("<color=#ff0000>会議で追放されること</color>が勝利条件です。");
            
            BillboardModule.Instance.AddBillboard(jester.Character!, "<color=#ff0000>てるてる</color>");
        }

        [EventListener]
        public async void OnPlayerExiled(IPlayerExileEvent e)
        {
            var session = Get(e.Game);
            if (session == null) return;

            var id = session.jester?.Client.Id;

            // てるてるの勝利
            if (id is int jesterId && jesterId == e.ClientPlayer.Client.Id)
            {
                session.jesterWon = true;
                var jester = e.Game.GetClientPlayer(jesterId)!;
                Logger.LogInformation("The Jester WON!");
                // てるてるに天使の輪をつける
                await jester.Character!.SetHatAsync(HatType.HaloHat);

                foreach (var player in e.Game.Players)
                {
                    if (player.Client.Id != jesterId)
                    {
                        await player.Character!.SetHatAsync(HatType.DumSticker);
                    }
                }
                foreach (var player in e.Game.Players)
                {
                    var c = player.Character!;
                    if (!c.PlayerInfo.IsDead && c.PlayerInfo.IsImpostor)
                    {
                        Logger.LogInformation($"{player.Character!.PlayerInfo.PlayerName} killed because the jester won");
                        await player.Character.MurderPlayerAsync(player.Character);
                    }
                }
            }
        }

        [EventListener]
        public void OnGameDestroyed(IGameDestroyedEvent e)
        {
            sessions.Remove(e.Game);
        }

        private JesterSession? Get(IGame game) => sessions.ContainsKey(game) ? sessions[game] : null;

        private readonly Dictionary<IGame, JesterSession> sessions = new();
    }
}