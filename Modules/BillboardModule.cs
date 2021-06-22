using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Impostor.Api.Innersloth;
using Impostor.Api.Net.Inner.Objects;
using Microsoft.Extensions.Logging;

namespace EbingUs
{
    public class BillboardModule : ModuleBase<BillboardModule>
    {

        public void AddBillboard(IInnerPlayerControl p, string text)
        {
            if (!data.ContainsKey(p)) CreateBillboard(p);
            data[p].DataQueue.Enqueue(text);
            Logger.LogInformation("Added a text '" + text + "' to a billboard of " + p.PlayerInfo.PlayerName);
        }

        public void CreateBillboard(IInnerPlayerControl p)
        {
            var q = new Queue<string>();
            q.Enqueue(p.PlayerInfo.PlayerName);
            data.Add(p, new BillboardData(p, q));
            Logger.LogInformation("Created a billboard of " + p.PlayerInfo.PlayerName);
        }

        public override void OnEnabled()
        {
            cts = new();
            Task.Run(Worker);
        }

        public override void OnDisabled()
        {
            cts.Cancel();
        }

        public async ValueTask Worker()
        {
            Logger.LogInformation("Started BillboardModule Worker Thread.");
            while (true)
            {
                await Task.WhenAll
                (
                    data.Values.ToList().Select(async d =>
                    {
                        var (player, queue) = d;
                        // 終了した試合のものであれば掃除
                        if (player.Game.GameState == GameStates.Destroyed || player.Game.GameState == GameStates.Ended)
                        {
                            Logger.LogInformation("Removed a billboard of " + player.PlayerInfo.PlayerName + ".");
                            data.Remove(player);
                        }
                        var text = queue.Dequeue();
                        await RpcUtility.SetPlayerLocalNameAsync(player, text);
                        queue.Enqueue(text);
                    })
                );
                await Task.Delay(1000, cts.Token);
                if (cts.IsCancellationRequested) break;
            }
        }

        record BillboardData(IInnerPlayerControl Player, Queue<string> DataQueue);

        private readonly Dictionary<IInnerPlayerControl, BillboardData> data = new();
        private CancellationTokenSource cts = new();
    }
}