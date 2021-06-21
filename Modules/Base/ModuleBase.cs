using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Impostor.Api.Events;
using Impostor.Api.Games;
using Impostor.Api.Net.Inner;
using Impostor.Api.Net.Inner.Objects;
using Microsoft.Extensions.Logging;

namespace EbingUs
{
    /// <summary>
    /// Ebing Us プラグインのモジュールの基底クラス
    /// </summary>
    public abstract class ModuleBase<T> : IModuleBase where T : ModuleBase<T>, new()
    {
        public static T Instance { get; }

        public ILogger<EbingUsPlugin> Logger => EbingUsPlugin.Instance.Logger;

        static ModuleBase()
        {
            Instance = new T();
        }

        public virtual void OnEnabled() { }
        public virtual void OnDisabled() { }

        protected ModuleBase() { }
    }
}