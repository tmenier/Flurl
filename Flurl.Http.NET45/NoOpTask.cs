using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flurl.Http
{
	internal static class NoOpTask
	{
		public static readonly Task Instance = Task.FromResult(0);
	}
}
