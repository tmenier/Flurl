using Flurl.Http.Configuration;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture, Parallelizable]
	public class DeserializationTests : HttpTestFixtureBase
	{
		[Test]
		public void can_be_deserialize_int_generic() {
			var serializer = new NewtonsoftJsonSerializer(null);
			var result = serializer.Deserialize<int>("1");
			Assert.IsNotNull(result);
			Assert.AreEqual(1, result);
		}

		[Test]
		public void can_be_deserialize_int_type() {
			var serializer = new NewtonsoftJsonSerializer(null);
			var result = serializer.Deserialize("1", typeof(int));
			Assert.IsNotNull(result);
			Assert.AreEqual(typeof(int), result.GetType());
			Assert.AreEqual(1, result);
		}

		[Test]
		public void can_be_deserialize_testdata_object_generic() {
			var serializer = new NewtonsoftJsonSerializer(null);
			var result = serializer.Deserialize<TestData>("{id: 1, name:\"jon\"}");
			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.id);
			Assert.AreEqual("jon", result.name);
		}

		[Test]
		public void can_be_deserialize_testdata_object_type() {
			var serializer = new NewtonsoftJsonSerializer(null);
			var result = serializer.Deserialize("{id: 1, name:\"jon\"}", typeof(TestData));
			Assert.IsNotNull(result);
			Assert.AreEqual(typeof(TestData), result.GetType());

			var resultTestdata = (TestData)result;
			Assert.AreEqual(1, resultTestdata.id);
			Assert.AreEqual("jon", resultTestdata.name);
		}

		private class TestData
		{
			public int id { get; set; }
			public string name { get; set; }
		}
	}
}