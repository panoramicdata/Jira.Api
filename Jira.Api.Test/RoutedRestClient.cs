using RestSharp.Authenticators;
using System.Net;

namespace Jira.Api.Test;

internal sealed class RoutedRestClient : JiraRestClient
{
	private readonly Func<Method, string, RestResponse> _route;

	public RoutedRestClient(Func<Method, string, RestResponse> route, JiraRestClientSettings? settings = null)
		: base("http://jira.test", (IAuthenticator?)null, settings ?? new JiraRestClientSettings("Test/1.0"))
		=> _route = route;

	public List<(Method Method, string Resource)> Calls { get; } = [];

	public static RestResponse Json(string content, HttpStatusCode status = HttpStatusCode.OK)
		=> new() { StatusCode = status, Content = content };

	public static (RoutedRestClient Client, JiraClient Jira) Create(string content, HttpStatusCode status = HttpStatusCode.OK, JiraRestClientSettings? settings = null)
		=> Create((_, _) => Json(content, status), settings);

	public static (RoutedRestClient Client, JiraClient Jira) Create(Func<Method, string, RestResponse> route, JiraRestClientSettings? settings = null)
	{
		var rest = new RoutedRestClient(route, settings);
		return (rest, JiraClient.CreateRestClient(rest, rest.Settings.Cache));
	}

	private static string Query(RestRequest request)
	{
		var pairs = request.Parameters
			.Where(p => p.Type is ParameterType.QueryString or ParameterType.GetOrPost)
			.Select(p => $"{p.Name}={p.Value}")
			.ToArray();
		return pairs.Length == 0 ? string.Empty : "?" + string.Join("&", pairs);
	}

	protected override Task<RestResponse> SendRequestAsync(RestRequest request, CancellationToken cancellationToken)
	{
		Calls.Add((request.Method, request.Resource + Query(request)));
		return Task.FromResult(_route(request.Method, request.Resource));
	}
}
