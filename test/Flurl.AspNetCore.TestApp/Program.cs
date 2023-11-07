using Flurl.AspNetCore.TestApp;
using Flurl.Http.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
	.AddTransient<TestService>()
	.AddTransient<MiddlewareDependency>()
	.AddTransient<TestMiddleware>();

builder.Services.AddSingleton<IFlurlClientCache>(sp => new FlurlClientCache()
	.WithDefaults(c => c.AddMiddleware(sp.GetService<TestMiddleware>))
	.Add("foo", "https://foo.com", null)
	.Add("bar", "https://bar.com", null));

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
