using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace Flurl.Http.Testing
{
	public class HttpTester
	{
		private static HttpResponseMessage _emptyResponse = new HttpResponseMessage {
			StatusCode = HttpStatusCode.OK,
			Content = new StringContent("")
		};

		public HttpTester() {
			Reset();
		}

		public HttpTester Reset() {
			FlurlHttp.TestMode = true;
			ResponseQueue = new Queue<HttpResponseMessage>();
			CallLog = new List<CallLogEntry>();
			return this;
		}

		public HttpTester RespondWith(int status, string body) {
			ResponseQueue.Enqueue(new HttpResponseMessage {
				StatusCode = (HttpStatusCode)status,
				Content = new StringContent(body)
			});
			return this;
		}

		public HttpTester RespondWith(string body) {
			return RespondWith(200, body);
		}

		public HttpTester RespondWithJson(int status, object data) {
			ResponseQueue.Enqueue(new HttpResponseMessage {
				StatusCode = (HttpStatusCode)status,
				Content = new JsonContent(data)
			});
			return this;
		}

		public HttpTester RespondWithJson(object data) {
			return RespondWithJson(200, data);
		}

		public Queue<HttpResponseMessage> ResponseQueue { get; set; }

		public HttpResponseMessage GetNextResponse() {
			return ResponseQueue.Any() ? ResponseQueue.Dequeue() : _emptyResponse;
		}
	
		public List<CallLogEntry> CallLog { get; private set; }
	}
}
