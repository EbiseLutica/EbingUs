using Impostor.Api.Events;

namespace EbingUs
{
    public class HelloModule : ModuleBase<HelloModule>
    {
        public static readonly string HELLO_MESSAGE = $"<b><i><color=#eb4034>Ebing Us!?</color>へようこそ！</i></b>\n<b>バージョン: </b>{EbingUsPlugin.VERSION}";

        [EventListener]
        public void onPlayerJoined(IGamePlayerJoinedEvent e)
        {
            _ = e.Player.Character?.SendChatToPlayerAsync(HELLO_MESSAGE);
        }
    }
}