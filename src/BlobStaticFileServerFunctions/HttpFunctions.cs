using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Specialized;
using BlobStaticFileServerFunctions.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BlobStaticFileServerFunctions
{
	public class HttpFunctions
	{
		private readonly BasicAuthOptions basicAuthOptions;
		
		public HttpFunctions(IOptions<BasicAuthOptions> basicAuthOptions)
		{
			this.basicAuthOptions = basicAuthOptions.Value;
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

		private bool ValidateAuthHeader(string base64AuthParameter)
		{
			if (basicAuthOptions.Bypass)
			{
				return true;
			}

			if (string.IsNullOrWhiteSpace(base64AuthParameter))
			{
				return false;
			}

			var bytes = Convert.FromBase64String(base64AuthParameter);
			var decodedAuthValue = UTF8Encoding.UTF8.GetString(bytes);

			return basicAuthOptions.Header.Equals(decodedAuthValue);
		}
	}
}
