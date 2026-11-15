using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp.Authenticators;
using RestSharp.Serializers.NewtonsoftJson;
using System.Diagnostics;
using System.Net;

namespace Jira.Api;

/// <summary>
/// Client for interacting with JIRA via REST API.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2360:Optional parameters should not be used", Justification = "Published public API: replacing optional parameters with overloads is a breaking change for existing consumers.")]
public partial class JiraRestClient : IJiraRestClient
{
	private void EnsureRequestAllowed(RestRequest request)
	{
		if (!Settings.IsReadOnly || request.Method is Method.Get or Method.Head or Method.Options || IsReadOnlySearch(request))
		{
			return;
		}

		throw new InvalidOperationException($"The client is read-only (IsReadOnly is true): {request.Method} {request.Resource} is not allowed.");
	}

	// POST rest/api/2/search is how JQL is sent; it only reads.
	private static bool IsReadOnlySearch(RestRequest request)
		=> request.Method == Method.Post
			&& request.Resource.Split('?')[0].TrimEnd('/').Equals("rest/api/2/search", StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Downloads data asynchronously from the specified URL.
	/// </summary>
	/// <remarks>This method performs an asynchronous HTTP GET request to retrieve the data from the specified URL.
	/// Ensure the URL is properly formatted and accessible.</remarks>
	/// <param name="url">The URL from which to download data. Must be a valid, non-empty URL.</param>
	/// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
	/// <returns>A byte array containing the downloaded data, or <see langword="null"/> if the download fails.</returns>
	public async Task<byte[]?> DownloadDataAsync(string url, CancellationToken cancellationToken)
	{
		// Synchronous download is not supported in modern RestSharp, use async and block if needed
		var request = new RestRequest(url, Method.Get);
		return await RestClient.DownloadDataAsync(request, cancellationToken);
	}

	/// <summary>
	/// Downloads data from the specified URL and saves it to the specified file.
	/// </summary>
	/// <remarks>This method downloads the data asynchronously and writes it to the specified file.  Ensure that the
	/// provided file path is valid and that the application has sufficient permissions to write to the file.</remarks>
	/// <param name="url">The URL from which to download the data. Must be a valid, accessible URL.</param>
	/// <param name="fullFileName">The full path and name of the file where the downloaded data will be saved.  The caller must ensure the path is
	/// valid and writable.</param>
	/// <param name="cancellationToken">A token that can be used to cancel the operation. If cancellation is requested,  the operation will terminate and
	/// no file will be written.</param>
	/// <returns>A task that represents the asynchronous download operation.</returns>
	public async Task DownloadAsync(string url, string fullFileName, CancellationToken cancellationToken)
	{
		var data = await DownloadDataAsync(url, cancellationToken);
		File.WriteAllBytes(fullFileName, data);
	}

	private void LogRequest(RestRequest request, object? body = null)
	{
		if (Settings.EnableRequestTrace)
		{
			Trace.WriteLine($"[{request.Method}] Request Url: {request.Resource}");

			if (body != null)
			{
				Trace.WriteLine($"[{request.Method}] Request Data: {body.GetType().Name} (body omitted from the trace)");
			}
		}
	}

	private JToken? GetValidJsonFromResponse(RestRequest request, RestResponse response)
	{
		var content = response.Content != null ? response.Content.Trim() : string.Empty;

		if (Settings.EnableRequestTrace)
		{
			Trace.WriteLine($"[{request.Method}] Response for Url: {request.Resource}\n{content}");
		}

		ValidateResponse(request, response, content);

		if (string.IsNullOrWhiteSpace(content))
		{
			return new JObject();
		}

		if (!content.StartsWith('{') && !content.StartsWith('['))
		{
			throw new InvalidOperationException($"Response was not recognized as JSON. Content: {content}");
		}

		return ParseJsonContent(content);
	}

	private static void ValidateResponse(RestRequest request, RestResponse response, string content)
	{
		if (!string.IsNullOrEmpty(response.ErrorMessage))
		{
			throw new InvalidOperationException($"Error Message: {response.ErrorMessage}");
		}

		if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
		{
			if (response.StatusCode == HttpStatusCode.Unauthorized && request.Resource.StartsWith("rest/api/2/workflowscheme", StringComparison.OrdinalIgnoreCase))
			{
				throw new WebSudoRequiredException($"A WebSudo session is required for '{request.Resource}'. Response Content: {content}");
			}

			throw new System.Security.Authentication.AuthenticationException($"Response Content: {content}");
		}

		if (response.StatusCode == HttpStatusCode.NotFound)
		{
			throw new ResourceNotFoundException($"Response Content: {content}");
		}

		if ((int)response.StatusCode >= 400)
		{
			throw new InvalidOperationException($"Response Status Code: {(int)response.StatusCode}. Response Content: {content}");
		}
	}

	private static JToken? ParseJsonContent(string content)
	{
		JToken parsedContent;

		try
		{
			parsedContent = JToken.Parse(content);
		}
		catch (JsonReaderException ex)
		{
			throw new InvalidOperationException($"Failed to parse response as JSON. Content: {content}", ex);
		}

		if (parsedContent != null && parsedContent.Type == JTokenType.Object && parsedContent["errorMessages"] != null)
		{
			throw new InvalidOperationException($"Response reported error(s) from JIRA: {parsedContent["errorMessages"]}");
		}

		return parsedContent;
	}
}
