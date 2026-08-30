using System.Collections;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Jira.Api.Test.Integration;

internal class JiraProvider : IEnumerable<object[]>
{
	// Fallback values for a local, throwaway Jira instance. Real credentials come from
	// configuration (environment variables or user secrets); see BuildConfiguration below.
	private const string DefaultHost = "http://localhost:8080";
	private const string DefaultUsername = "admin";
	private const string DefaultPassword = "admin";

	// Public values consumed by tests (kept same names for backwards compatibility)
	public static readonly string HOST;
	public static readonly string USERNAME;
	public static readonly string PASSWORD;
	public static readonly string OAUTHCONSUMERKEY;
	public static readonly string OAUTHCONSUMERSECRET;
	public static readonly string OAUTHACCESSTOKEN;
	public static readonly string OAUTHTOKENSECRET;
	public static readonly bool HasOAuthConfiguration;

	private static readonly JiraClient _jiraWithCredentials;
	private static readonly JiraClient? _jiraWithOAuth;

	private readonly List<object[]> _data;

	static JiraProvider()
	{
		var configuration = BuildConfiguration();

		// Map configuration keys (environment or user secrets override json)
		HOST = GetConfig(configuration, "Jira:Host", DefaultHost);
		USERNAME = GetConfig(configuration, "Jira:Username", DefaultUsername);
		PASSWORD = GetConfig(configuration, "Jira:Password", DefaultPassword);
		OAUTHCONSUMERKEY = GetOptionalConfig(configuration, "Jira:OAuth:ConsumerKey") ?? string.Empty;
		OAUTHCONSUMERSECRET = GetOptionalConfig(configuration, "Jira:OAuth:ConsumerSecret") ?? string.Empty;
		OAUTHACCESSTOKEN = GetOptionalConfig(configuration, "Jira:OAuth:AccessToken") ?? string.Empty;
		OAUTHTOKENSECRET = GetOptionalConfig(configuration, "Jira:OAuth:TokenSecret") ?? string.Empty;
		HasOAuthConfiguration = AllPresent(OAUTHCONSUMERKEY, OAUTHCONSUMERSECRET, OAUTHACCESSTOKEN, OAUTHTOKENSECRET);

		_jiraWithCredentials = JiraClient.CreateRestClient(HOST, USERNAME, PASSWORD);

		if (HasOAuthConfiguration)
		{
			_jiraWithOAuth = JiraClient.CreateOAuthRestClient(
				HOST,
				OAUTHCONSUMERKEY,
				OAUTHCONSUMERSECRET,
				OAUTHACCESSTOKEN,
				OAUTHTOKENSECRET);
		}
	}

	private static bool AllPresent(params string[] values) => Array.TrueForAll(values, v => !string.IsNullOrWhiteSpace(v));

	public JiraProvider()
	{
		_data = [[_jiraWithCredentials]];

		if (_jiraWithOAuth != null)
		{
			_data.Add([_jiraWithOAuth]);
		}
	}

	public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	private static IConfigurationRoot BuildConfiguration() =>
		new ConfigurationBuilder()
			.SetBasePath(AppContext.BaseDirectory)
			.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
			.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
			.AddEnvironmentVariables()
			.Build();

	private static string GetConfig(IConfiguration config, string key, string fallback)
	{
		var value = config[key];
		return string.IsNullOrWhiteSpace(value) ? fallback : value;
	}

	private static string? GetOptionalConfig(IConfiguration config, string key)
	{
		var value = config[key];

		if (string.IsNullOrWhiteSpace(value))
		{
			return null;
		}

		var trimmedValue = value.Trim();
		return trimmedValue.StartsWith('<') && trimmedValue.EndsWith('>') ? null : trimmedValue;
	}
}



