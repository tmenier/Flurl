using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace Flurl.Http
{
	public class JsonContent : StringContent
	{
		public JsonContent(object data) : base(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json") { }
	}
}
