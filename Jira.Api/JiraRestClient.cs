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
	/// <summary>
	/// Authenticates to the JIRA server using the provided URL, username, and password.
	/// </summary>
	/// <param name="url"></param>
	/// <param name="username"></param>
	/// <param name="password"></param>
	/// <param name="settings"></param>
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

	/// <summary>
	/// Authenticates to the JIRA server using the provided URL and Personal Access Token, which is used in the authentication header as "Bearer TOKEN".
	/// </summary>
	/// <param name="url"></param>
	/// <param name="personalAccessToken"></param>
	/// <param name="settings"></param>
	public JiraRestClient(
		string url,
		string personalAccessToken,
		JiraRestClientSettings? settings = null)
		: this(
			url,
			new JwtAuthenticator(personalAccessToken),
			settings)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="JiraRestClient"/> class with the specified URL, authenticator, and optional settings.
	/// </summary>
	/// <param name="url">The base URL of the JIRA server.</param>
	/// <param name="authenticator">The authenticator to use for requests, or <see langword="null"/> for anonymous access.</param>
	/// <param name="settings">Optional settings for the REST client.</param>
	protected JiraRestClient(
		string url,
		IAuthenticator? authenticator,
		JiraRestClientSettings? settings = null)
	{
		url = url.EndsWith('/') ? url : url + "/";
#pragma warning disable CS0618 // Intentional fallback to default settings within the library
		Settings = settings ?? new JiraRestClientSettings();
#pragma warning restore CS0618

		RestClient = new RestClient(new RestClientOptions(url)
		{
			Proxy = Settings.Proxy,
			Authenticator = authenticator,
			UserAgent = Settings.UserAgent,
		}, configureSerialization: s => s.UseNewtonsoftJson());
	}

	/// <inheritdoc/>
	public RestClient RestSharpClient => RestClient;

	/// <inheritdoc/>
	public string Url => RestClient.Options.BaseUrl.ToString();

	/// <inheritdoc/>
	public JiraRestClientSettings Settings { get; }

	/// <summary>
	/// Gets the underlying RestSharp client.
	/// </summary>
	public RestClient RestClient { get; }
}
