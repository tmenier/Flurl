﻿#if PORTABLE
using System.Threading.Tasks;

namespace Flurl.Http
{
	internal static class NoOpTask
	{
		public static readonly Task Instance = TaskEx.FromResult(0);
	}
}
#endif