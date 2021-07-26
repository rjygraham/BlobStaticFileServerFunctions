using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Specialized;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace BlobStaticFileServerFunctions
{
	public class HttpFunctions
	{
		private readonly bool bypassBasicAuth;
		private readonly string expectedBasicAuthValue;

		public HttpFunctions()
		{
			bool.TryParse(Environment.GetEnvironmentVariable("BASIC_BYPASS"), out bypassBasicAuth);

			if (!bypassBasicAuth)
			{
				expectedBasicAuthValue = $"{Environment.GetEnvironmentVariable("BASIC_USERNAME")}:{Environment.GetEnvironmentVariable("BASIC_PASSWORD")}";
			}
		}

		[FunctionName(nameof(ServeContent))]
		public async Task<IActionResult> ServeContent(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{*fileName}")] HttpRequestMessage req,
			[Blob("%STATIC_CONTAINER%/{fileName}", FileAccess.Read, Connection = "STATIC")] BlockBlobClient file,
			ILogger log)
		{
			log.LogDebug($"Authenticating.");
			if (req.Headers.Authorization == null || !ValidateAuthHeader(req.Headers.Authorization.Parameter))
			{
				return new UnauthorizedResult();
			}

			log.LogDebug($"Checking file existence: {file.Uri}");
			if (!(await file.ExistsAsync()).Value)
			{
				return new NotFoundResult();
			}

			log.LogDebug($"Downloading file: {file.Uri}");
			var downloadStream = await file.DownloadStreamingAsync();

			log.LogDebug($"Returning file with content-type: {downloadStream.Value.Details.ContentType}");
			return new FileStreamResult(downloadStream.Value.Content, downloadStream.Value.Details.ContentType);
		}

		private bool ValidateAuthHeader(string authParameter)
		{
			if (bypassBasicAuth)
			{
				return true;
			}

			if (string.IsNullOrWhiteSpace(authParameter))
			{
				return false;
			}

			var bytes = Convert.FromBase64String(authParameter);
			var actualAuthValue = UTF8Encoding.UTF8.GetString(bytes);

			return expectedBasicAuthValue.Equals(actualAuthValue);
		}
	}
}
