using BlobStaticFileServerFunctions;
using BlobStaticFileServerFunctions.Options;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace BlobStaticFileServerFunctions
{
	public class Startup : FunctionsStartup
	{
		public override void Configure(IFunctionsHostBuilder builder)
		{
			builder.Services.AddOptions<BasicAuthOptions>()
			.Configure<IConfiguration>((settings, configuration) =>
			{
				configuration.GetSection("BASICAUTH").Bind(settings);
			});
		}
	}
}
