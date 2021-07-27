# BlobStaticFileServerFunctions

A simple Azure Function that serves static files from Azure Blob Storage and supports Basic Authentication. This is useful for scenarios where static files need to be protected with Basic Authentication. OAuth can be supported by leveraging Azure Functions [built-in authentication and authorization](https://docs.microsoft.com/en-us/azure/app-service/overview-authentication-authorization) but if you go down this path, you might consider [Azure Static Web Apps](https://docs.microsoft.com/en-us/azure/static-web-apps/authentication-authorization) instead.

## Azure Setup

Setup for this solution is fairly straight forward:

1. Create an Azure Function, associated Storage Account, and optional Application Insights resources
2. RECOMMENDED: Create separate Storage Account and blob container for hosting the static files.
3. Publish the function code to the Azure Function
4. Configure the Azure Function App Settings per instructions below.
5. Upload content to the Storage Account ensuring you set the Content-Type attribute as this will be used by the function to set the return Content-Type header in the response.

### Basic Authentication

To enable Basic Authentication, add the following App Settings to your function:

```bash
BASICAUTH__USERNAME: "Your Username Value"
BASICAUTH__PASSWORD: "Your Password Value"
BASICAUTH__BYPASS: "false"
```

To disable Basic Authentication and rely on [built-in authentication and authorization](https://docs.microsoft.com/en-us/azure/app-service/overview-authentication-authorization) or disable authentication all together, add the following App Setting to your function:

```bash
BASICAUTH__BYPASS: "true"
```

### Azure Function Managed Identity Auth to Storage Account (RECOMMENDED)

To configure the [Azure Function to access the Storage Account via Managed Identity](https://docs.microsoft.com/en-us/azure/azure-functions/functions-reference#configure-an-identity-based-connection) be sure to enable the Azure Function Managed Identity and grant the identity Storage Blob Data Reader RBAC to the blob container. Once complete, add the following App Settings to your function:

```bash
STATIC__serviceUri: "https://[yourstorageaccount].blob.core.windows.net/"
STATIC_CONTAINER: "static_content_container_name"
```
> NOTE: STATIC__serviceUri contains two underscores and STATIC_CONTAINER contains a single underscore.

### Azure Function SAS or Account Key Auth to Storage Account

To configure the Azure Function to access the Storage Account via SAS or Account Key, add the following App Settings to your function:

```bash
STATIC: "SAS or Account Key ConnectionString"
STATIC_CONTAINER: "static_content_container_name"
```
### Localhost Setup

Local setup will require a `local.settings.json` file to be created in the same folder as the `host.json` file. The file will contain a combination of the following App Settings depending on your environment configuration. Please read the Azure Setup section for more details.

```json
{
	"IsEncrypted": false,
	"Values": {
		"AzureWebJobsStorage": "UseDevelopmentStorage=true",
		"FUNCTIONS_WORKER_RUNTIME": "dotnet",
		"STATIC": "",
		"STATIC__serviceUri": "",
		"STATIC__tenantId": "", // May be required for local development if using Managed Identities
		"STATIC_CONTAINER": "",
		"BASICAUTH__USERNAME": "",
		"BASICAUTH__PASSWORD": "",
		"BASICAUTH__BYPASS": ""
	}
}
```

## Usage

After successful setup, you should be able to issue a request such as the following and get a successful to 200 response:

### Azure
```http
GET https://[yourfunctionappname].azurewebsites.net/index.html
authorization: Basic Zm9vOmJhcg==
```

### Localhost
```http
GET http://localhost:7071/index.html
authorization: Basic Zm9vOmJhcg==
```
Other possible repsonse codes include:

```
401 - Unauthorized = Authorization header did not match configured values
404 - Not Found = File does't exist in blob.
500 - Internal Server Error = there was an error with setup or something else went wrong, please open a GH issue
```

## License

The MIT License (MIT)

Copyright © 2021 Ryan Graham

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.