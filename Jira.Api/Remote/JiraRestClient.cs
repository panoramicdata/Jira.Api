using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api.Remote;

/// <summary>
/// Client for interacting with JIRA via REST API.
/// </summary>
public class JiraRestClient : IJiraRestClient
{
	private readonly RestClient _restClient;
	private readonly JiraRestClientSettings _clientSettings;

	public JiraRestClient(
		string url,
		string? username = null,
		string? password = null,
		JiraRestClientSettings? settings = null)
		: this(
			url,
			!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password)
				? new HttpBasicAuthenticator(username, password)
				: null,
			settings)
	{
	}

	protected JiraRestClient(
		string url,
		IAuthenticator? authenticator,
		JiraRestClientSettings? settings = null)
	{
		url = url.EndsWith('/') ? url : url += "/";
		_clientSettings = settings ?? new JiraRestClientSettings();

		_restClient = new RestClient(new RestClientOptions(url)
		{
			Proxy = _clientSettings.Proxy,
			Authenticator = authenticator,

		}, configureSerialization: s => s.UseNewtonsoftJson());

	}

	public RestClient RestSharpClient => RestClient;

	public string Url => RestClient.Options.BaseUrl.ToString();

	public JiraRestClientSettings Settings => _clientSettings;

	public RestClient RestClient => _restClient;

	public async Task<T?> ExecuteRequestAsync<T>(Method method, string resource, object? requestBody, CancellationToken cancellationToken)
	{
		var result = await ExecuteRequestAsync(method, resource, requestBody, cancellationToken).ConfigureAwait(false);
		return JsonConvert.DeserializeObject<T>(result.ToString(), Settings.JsonSerializerSettings);
	}

	/// <summary>
	/// Executes an HTTP request asynchronously using the specified method, resource, and optional request body.
	/// </summary>
	/// <remarks>This method performs logging of the request and handles serialization of the request body. It also
	/// validates the response to ensure it contains valid JSON.</remarks>
	/// <param name="method">The HTTP method to use for the request, such as <see cref="Method.Get"/> or <see cref="Method.Post"/>.</param>
	/// <param name="resource">The resource endpoint to target, typically a relative URL.</param>
	/// <param name="requestBody">An optional object representing the request body. If the body is a string, it is sent as raw JSON. If the body is a
	/// non-string object, it is serialized as JSON. This parameter is ignored for GET requests.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
	/// <returns>A <see cref="JToken"/> representing the parsed JSON response, or <see langword="null"/> if the response does not
	/// contain valid JSON.</returns>
	/// <exception cref="InvalidOperationException">Thrown if <paramref name="method"/> is <see cref="Method.Get"/> and <paramref name="requestBody"/> is not <see
	/// langword="null"/>.</exception>
	public async Task<JToken?> ExecuteRequestAsync(
		Method method,
		string resource,
		object? requestBody,
		CancellationToken cancellationToken)
	{
		if (method == Method.Get && requestBody != null)
		{
			throw new InvalidOperationException($"GET requests are not allowed to have a request body. Resource: {resource}. Body: {requestBody}");
		}

		var request = new RestRequest(resource, method);

		if (requestBody is string)
		{
			request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
		}
		else if (requestBody != null)
		{
			request.AddJsonBody(requestBody);
		}

		LogRequest(request, requestBody);
		var response = await ExecuteRawResquestAsync(request, cancellationToken).ConfigureAwait(false);

		return GetValidJsonFromResponse(request, response);
	}

	/// <summary>
	/// Executes the specified REST request asynchronously and returns the response.
	/// </summary>
	/// <remarks>This method logs the request, executes it, and processes the response to ensure valid JSON. The
	/// caller is responsible for handling any exceptions that may occur during execution.</remarks>
	/// <param name="request">The REST request to execute. Must not be <see langword="null"/>.</param>
	/// <param name="cancellationToken">An optional cancellation token that can be used to cancel the operation.</param>
	/// <returns>A <see cref="RestResponse"/> object containing the result of the executed request.</returns>
	public async Task<RestResponse> ExecuteRequestAsync(
		RestRequest request,
		CancellationToken cancellationToken)
	{
		LogRequest(request);
		var response = await ExecuteRawResquestAsync(request, cancellationToken).ConfigureAwait(false);
		GetValidJsonFromResponse(request, response);
		return response;
	}

	/// <summary>
	/// Executes the specified REST request asynchronously and returns the raw response.
	/// </summary>
	/// <remarks>This method is designed to execute a REST request using the underlying REST client and return the
	/// raw response. It does not perform any additional processing or validation on the response.</remarks>
	/// <param name="request">The REST request to execute. Must not be <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
	/// <returns>A task representing the asynchronous operation. The task result contains the raw response from the REST client.</returns>
	protected virtual Task<RestResponse> ExecuteRawResquestAsync(RestRequest request, CancellationToken cancellationToken)
	{
		return RestClient.ExecuteAsync(request, cancellationToken);
	}

	/// <summary>
	/// Downloads data asynchronously from the specified URL.
	/// </summary>
	/// <remarks>This method performs an asynchronous HTTP GET request to retrieve the data from the specified URL.
	/// Ensure the URL is properly formatted and accessible.</remarks>
	/// <param name="url">The URL from which to download data. Must be a valid, non-empty URL.</param>
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
		if (_clientSettings.EnableRequestTrace)
		{
			Trace.WriteLine($"[{request.Method}] Request Url: {request.Resource}");

			if (body != null)
			{
				Trace.WriteLine($"[{request.Method}] Request Data: {JsonConvert.SerializeObject(body, new JsonSerializerSettings()
				{
					Formatting = Formatting.Indented,
					NullValueHandling = NullValueHandling.Ignore
				})}");
			}
		}
	}

	private JToken? GetValidJsonFromResponse(RestRequest request, RestResponse response)
	{
		var content = response.Content != null ? response.Content.Trim() : string.Empty;

		if (_clientSettings.EnableRequestTrace)
		{
			Trace.WriteLine($"[{request.Method}] Response for Url: {request.Resource}\n{content}");
		}

		if (!string.IsNullOrEmpty(response.ErrorMessage))
		{
			throw new InvalidOperationException($"Error Message: {response.ErrorMessage}");
		}
		else if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
		{
			throw new System.Security.Authentication.AuthenticationException($"Response Content: {content}");
		}
		else if (response.StatusCode == HttpStatusCode.NotFound)
		{
			throw new ResourceNotFoundException($"Response Content: {content}");
		}
		else if ((int)response.StatusCode >= 400)
		{
			throw new InvalidOperationException($"Response Status Code: {(int)response.StatusCode}. Response Content: {content}");
		}
		else if (string.IsNullOrWhiteSpace(content))
		{
			return new JObject();
		}
		else if (!content.StartsWith('{') && !content.StartsWith('['))
		{
			throw new InvalidOperationException($"Response was not recognized as JSON. Content: {content}");
		}
		else
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
}