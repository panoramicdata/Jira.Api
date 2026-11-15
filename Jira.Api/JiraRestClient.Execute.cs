using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp.Authenticators;
using RestSharp.Serializers.NewtonsoftJson;
using System.Diagnostics;
using System.Net;

namespace Jira.Api;

public partial class JiraRestClient
{
	/// <inheritdoc/>
	public async Task<T?> ExecuteRequestAsync<T>(
		Method method,
		string resource,
		object? requestBody, CancellationToken cancellationToken)
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

	/// <inheritdoc/>
	public async Task<RestResponse> ExecuteRawRequestAsync(RestRequest request, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(request);
		LogRequest(request);
		return await ExecuteRawResquestAsync(request, cancellationToken).ConfigureAwait(false);
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
		EnsureRequestAllowed(request);
		return SendRequestAsync(request, cancellationToken);
	}

	/// <summary>
	/// Sends the request to the server. Only reached once <see cref="JiraRestClientSettings.IsReadOnly"/> has been checked.
	/// </summary>
	/// <param name="request">The REST request to send.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
	protected virtual Task<RestResponse> SendRequestAsync(RestRequest request, CancellationToken cancellationToken)
	{
		return RestClient.ExecuteAsync(request, cancellationToken);
	}
}
