using Microsoft.AspNetCore.Mvc;

namespace Flurl.AspNetCore.TestApp.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class TestController : ControllerBase
	{
		private readonly TestService _testService;

		public TestController(TestService testService) {
			_testService = testService;
		}

		[HttpGet]
		public Task<string> Get() {
			return _testService.CallServiceAsync();
		}
	}
}