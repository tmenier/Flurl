using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Flurl;
using Flurl.Http;
using Flurl.Http.Testing;

namespace PackageTester
{
	public class Tester
	{
		private int _pass;
		private int _fail;

		public async Task DoTestsAsync() {
			_pass = 0;
			_fail = 0;

			await Test("Testing real request to google.com...", async () => {
				var real = await "http://www.google.com".GetStringAsync();
				Assert(real.Trim().StartsWith("<"), $"Response from google.com doesn't look right: {real}");
			});

			await Test("Testing fake request with HttpTest...", async () => {
				using (var test = new HttpTest()) {
					test.RespondWith("fake response");
					var fake = await "http://www.google.com".GetStringAsync();
					Assert(fake == "fake response", $"Fake response doesn't look right: {fake}");
				}
			});

			await Test("Testing file download...", async () => {
				var path = "c:\\google.txt";
				if (File.Exists(path)) File.Delete(path);
				var result = await "http://www.google.com".DownloadFileAsync("c:\\", "google.txt");
				Assert(result == path, $"Download result {result} doesn't match {path}");
				Assert(File.Exists(path), $"File didn't appear to download to {path}");
				if (File.Exists(path)) File.Delete(path);
			});

			if (_fail > 0) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"{_pass} passed, {_fail} failed");
			}
			else {
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Everything looks good");
			}
			Console.ResetColor();
		}

		private async Task Test(string msg, Func<Task> act) {
			Console.WriteLine(msg);
			try {
				await act();
				Console.WriteLine("pass.");
				_pass++;
			}
			catch (Exception ex) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Fail! {ex.Message}");
				Console.WriteLine(ex.StackTrace);
				_fail++;
			}
			finally {
				Console.ResetColor();
			}
		}

		private void Assert(bool check, string msg) {
			if (!check) throw new Exception(msg);
		}
	}

	internal class TestResponse
	{
		public string TestString { get; set; }
	}
}