using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Jira.Api.Test.Integration;

internal class JiraProvider : IEnumerable<object[]>
{
	// Default (legacy hard-coded) values kept for fallback.
	private const string DefaultHost = "http://localhost:8080";
	private const string DefaultUsername = "admin";
	private const string DefaultPassword = "admin";
	private const string DefaultOAuthConsumerKey = "JiraSdkConsumerKey";
	private const string DefaultOAuthConsumerSecret = "<RSAKeyValue><Modulus>odq47HoOGrM8b4FsbcaD+RpBCP1tNsKOcnH5wVd0XmgkEsiOaGiSUx1r9EEk9bBw2/Hq3DAXqbswOQOVt9WOPM27bjKDFJ2fkhFok//I3Wnsv/ZBpCHfyCT4dr9n8vd2HNVbDS4VqDNmoBbVs1Efkgw8ybcgsmGqqT7WZYmmSa8=</Modulus><Exponent>AQAB</Exponent><P>1HIXunp5C8dntdrNhOItLYYexBHDPSPCemdTLcoGIPrdGs+YgNHwpzjKAa89EToguijzxUigiZXbXLsimy+Whw==</P><Q>wwlrDSX0kV8b4jA5qDsoWN33h8BWotHq2YtajY2AyB7/MwmoBtWasQB1SxJcrILetbOqiTzJaZNdmQqg9RHVmQ==</Q><DP>abZYLlexEfZomepFqCDvwB5kAsaf8zVvGX9+uWM0x4ZtLWEtjrRo3pz4j/wGFCNrk5a7LmkkUTI7lJod70Cv0w==</DP><DQ>lxx/9eL3d36iIwDMW1ziaOApveM2/NX5yO2gjlYZdnQVtByCNDFhtkwtlKm4ZezL0ypOMiCHySXlegLzLI3R2Q==</DQ><InverseQ>mxXqH+teLS/8SgdBDi6cs5huMwXe7zAz33noZeiyi7Xm2ciyjvheCGFF201wBXehUemxMqmLGTCLWMBp0qsmnA==</InverseQ><D>VYeHgS9elK1ymloCOmBVDSXaiC2jsPRO4htop8rXK6xMo8BnwLTB3joF+iUSquJ6QUAto/2mA4NvkDFcxLCNYKziSj1JWIbfcc6gqPIKwtxyM3ZlSuJaG6GpNPh41SEhjtgMt2Cbf5Qy/prK1FkWFfOcvlOg+z2qGPQDXhS0QIE=</D></RSAKeyValue>";
	private const string DefaultOAuthAccessToken = "ZGUlzyOnuzS929YgIXv6Yt0TiZ8KbUAG";
	private const string DefaultOAuthTokenSecret = "EDeTxUt7QqDkoawenPY3QCaGeVGXa1BJ";

	// Public values consumed by tests (kept same names for backwards compatibility)
	public static readonly string HOST;
	public static readonly string USERNAME;
	public static readonly string PASSWORD;
	public static readonly string OAUTHCONSUMERKEY;
	public static readonly string OAUTHCONSUMERSECRET;
	public static readonly string OAUTHACCESSTOKEN;
	public static readonly string OAUTHTOKENSECRET;

	private static JiraClient _jiraWithCredentials = null!;
	private static JiraClient _jiraWithOAuth = null!;

	private readonly List<object[]> _data;

	static JiraProvider()
	{
		var configuration = BuildConfiguration();

		// Map configuration keys (environment or user secrets override json)
		HOST = GetConfig(configuration, "Jira:Host", DefaultHost);
		USERNAME = GetConfig(configuration, "Jira:Username", DefaultUsername);
		PASSWORD = GetConfig(configuration, "Jira:Password", DefaultPassword);
		OAUTHCONSUMERKEY = GetConfig(configuration, "Jira:OAuth:ConsumerKey", DefaultOAuthConsumerKey);
		OAUTHCONSUMERSECRET = GetConfig(configuration, "Jira:OAuth:ConsumerSecret", DefaultOAuthConsumerSecret);
		OAUTHACCESSTOKEN = GetConfig(configuration, "Jira:OAuth:AccessToken", DefaultOAuthAccessToken);
		OAUTHTOKENSECRET = GetConfig(configuration, "Jira:OAuth:TokenSecret", DefaultOAuthTokenSecret);

		_jiraWithCredentials = JiraClient.CreateRestClient(HOST, USERNAME, PASSWORD);
		_jiraWithOAuth = JiraClient.CreateOAuthRestClient(
			HOST,
			OAUTHCONSUMERKEY,
			OAUTHCONSUMERSECRET,
			OAUTHACCESSTOKEN,
			OAUTHTOKENSECRET);
	}

	public JiraProvider()
	{
		_data =
			[
				[_jiraWithCredentials],
				[_jiraWithOAuth]
			];
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
}
