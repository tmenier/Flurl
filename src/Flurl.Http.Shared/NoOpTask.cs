#if NETSTD
using System.Threading.Tasks;

namespace Flurl.Http
{
	internal static class NoOpTask
	{
		public static readonly Task Instance = Task.FromResult(0);
	}
}
#endif