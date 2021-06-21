using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Impostor.Api.Events.Managers;
using Impostor.Api.Plugins;
using Microsoft.Extensions.Logging;

namespace EbingUs
{
    /// <summary>
    /// プラグインの中心部。いろいろ定義しています。
    /// </summary>
    [ImpostorPlugin("work.xeltica.ebingus")]
    public class EbingUsPlugin : PluginBase
    {
        // プラグイン読み込み時にNonNullが確定するけど、コンパイラ的にそれを保証できないので仕方なく
        #pragma warning disable CS8618

        public static EbingUsPlugin Instance { get; private set; }

        public ILogger<EbingUsPlugin> Logger => logger;

        #pragma warning restore

        /// <summary>
        /// Impostor Serverが実行するコンストラクタです。
        /// </summary>
        public EbingUsPlugin(ILogger<EbingUsPlugin> logger, IEventManager eventManager)
        {
            this.logger = logger;
            this.eventManager = eventManager;
            Instance = this;
        }

        /// <summary>
        /// プラグインが有効化されたときに呼ばれます。
        /// </summary>
        public override ValueTask EnableAsync()
        {
            var mods = new IModuleBase[] {
                JesterModule.Instance,
                CommandModule.Instance,
            };

            // モジュールを読み込む
            foreach (var mod in mods)
            {
                logger.LogInformation($"Loading Module '{mod.GetType().Name}'");
                var disposable = eventManager.RegisterListener(mod);
                modules.Add((mod, disposable));
            }

            // モジュールの有効化
            foreach (var mod in mods)
            {
                logger.LogInformation($"Enabling Module '{mod.GetType().Name}'");
                mod.OnEnabled();
            }

            logger.LogInformation("Hello, Ebing Us.");
            return default;
        }


        /// <summary>
        /// プラグインが無効化されたときに呼ばれます。
        /// </summary>
        public override ValueTask DisableAsync()
        {
            // モジュールの無効化
            foreach (var (mod, _) in modules)
            {
                logger.LogInformation($"Disabling Module '{mod.GetType().Name}'");
                mod.OnDisabled();
            }

            // モジュールの削除
            foreach (var (mod, disposable) in modules)
            {
                logger.LogInformation($"Unloaing Module '{mod.GetType().Name}'");
                disposable.Dispose();
            }

            logger.LogInformation("Goodbye, Ebing Us.");
            return default;
        }

        private readonly ILogger<EbingUsPlugin> logger;
        private readonly IEventManager eventManager;
        private readonly List<(IModuleBase, IDisposable)> modules = new();
    }
}