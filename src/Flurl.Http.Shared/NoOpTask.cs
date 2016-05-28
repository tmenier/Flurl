using System.Threading.Tasks;

namespace Flurl.Http
{
    internal static class NoOpTask
    {
#if NETSTANDARD1_4 || NET45
        public static readonly Task Instance = Task.FromResult(0);
#elif PORTABLE
        public static readonly Task Instance = TaskEx.FromResult(0);
#endif
    }
}