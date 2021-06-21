using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace EbingUs
{
    /// <summary>
    /// 全日本 Task#ConfigureAwait(false) を許すな協会謹製つよつよ拡張機能
    /// </summary>
    public static class TaskExtension
    {
        public static ConfiguredTaskAwaitable Stay(this Task t) => t.ConfigureAwait(false);
        public static ConfiguredTaskAwaitable<T> Stay<T>(this Task<T> t) => t.ConfigureAwait(false);
        public static ConfiguredValueTaskAwaitable Stay(this ValueTask t) => t.ConfigureAwait(false);
        public static ConfiguredValueTaskAwaitable<T> Stay<T>(this ValueTask<T> t) => t.ConfigureAwait(false);
    }
}
