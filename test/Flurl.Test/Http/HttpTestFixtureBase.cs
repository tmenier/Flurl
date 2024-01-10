using System;
using System.Collections.Generic;
using System.Text;
using Flurl.Http.Testing;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	public abstract class HttpTestFixtureBase
	{
		protected HttpTest HttpTest { get; private set; }

		[SetUp]
		public void SetUp() {
			HttpTest = new HttpTest();
		}

		[TearDown]
		public void TearTown() {
			HttpTest.Dispose();
		}
	}
}
