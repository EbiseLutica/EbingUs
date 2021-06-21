using Impostor.Api.Events;

namespace EbingUs
{
    /// <summary>
    /// モジュールのインターフェイス。
    /// </summary>
    public interface IModuleBase : IEventListener
    {
        void OnDisabled();
        void OnEnabled();
    }
}