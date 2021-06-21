using System.Threading.Tasks;
using Impostor.Api.Games;
using Impostor.Api.Net.Inner;
using Impostor.Api.Net.Inner.Objects;

namespace EbingUs
{
    public static class RpcUtility
    {
        /// <summary>
        /// 対象のプレイヤーの名前を、そのプレイヤーのクライアントにおいてのみ変更します。
        /// </summary>
        public static async ValueTask SetPlayerLocalNameAsync(IGame game, IInnerPlayerControl target, string name)
        {
            var writer = game.StartRpc(target.NetId, RpcCalls.SetName, null);
            writer.Write(name);
            await game.FinishRpcAsync(writer, target.OwnerId);
        }
    }
}