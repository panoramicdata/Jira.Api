using Newtonsoft.Json;
using System.Net;
using System.Text.RegularExpressions;

namespace Jira.Api.Models;

/// <summary>
/// Settings to configure the JIRA REST client.
/// </summary>
public partial class JiraRestClientSettings
{
	/// <summary>
	/// The default User-Agent string used when none is explicitly provided.
	/// </summary>
	public const string DefaultUserAgent = "JiraApi_Application";

	[GeneratedRegex(@"^[A-Za-z0-9][A-Za-z0-9.\-_~]*(\/[A-Za-z0-9][A-Za-z0-9.\-_~]*)?( [A-Za-z0-9][A-Za-z0-9.\-_~]*(\/[A-Za-z0-9][A-Za-z0-9.\-_~]*)?)*$")]
	private static partial Regex UserAgentFormatRegex();


	private static IEnumerable<JsonConverter> _defaultJsonConverters =
		[
			new JiraUserJsonConverter()
		];

	private static IEnumerable<JsonConverter> _gdprJsonConverters =
		[
			new JiraUserJsonConverter() { UserPrivacyEnabled = true },
		];

	/// <summary>
	/// Whether to trace each request.
	/// </summary>
	public bool EnableRequestTrace { get; set; }

	/// <summary>
	/// Dictionary of serializers for custom fields.
	/// </summary>
	public IDictionary<string, ICustomFieldValueSerializer> CustomFieldSerializers { get; set; } = new Dictionary<string, ICustomFieldValueSerializer>();

	/// <summary>
	/// Cache to store frequently accessed server items.
	/// </summary>
	public JiraCache Cache { get; set; } = new JiraCache();

	/// <summary>
	/// The json global serializer settings to use.
	/// </summary>
	public JsonSerializerSettings JsonSerializerSettings { get; private set; } = new JsonSerializerSettings();

	/// <summary>
	/// Proxy to use when sending requests.
	/// </summary>
	/// <example>To enable debugging with Fiddler, set Proxy to new WebProxy("127.0.0.1", 8888)</example>
	public IWebProxy Proxy { get; set; }

	/// <summary>
	/// The User-Agent string sent with each HTTP request.
	/// </summary>
	public string UserAgent { get; }

	/// <summary>
	/// Whether to enable user privacy mode when interacting with Jira server (also known as GDPR mode).
	/// </summary>
	public bool EnableUserPrivacyMode
	{
		get;
		set
		{
			field = value;

			UpdateSerializers();
		}
	}

	/// <summary>
	/// Create a new instance of the settings with the specified User-Agent string.
	/// </summary>
	/// <param name="userAgent">The User-Agent product token to send with each request.
	/// Must conform to the RFC 9110 product token format (e.g. "MyApp/1.0").</param>
	/// <exception cref="ArgumentException">Thrown when <paramref name="userAgent"/> is null or whitespace.</exception>
	/// <exception cref="FormatException">Thrown when <paramref name="userAgent"/> does not match the expected product token format.</exception>
	public JiraRestClientSettings(string userAgent)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(userAgent);

		if (!UserAgentFormatRegex().IsMatch(userAgent))
		{
			throw new FormatException(
				$"The UserAgent value '{userAgent}' is not a valid RFC 9110 product token. " +
				"Expected format: 'ProductName' or 'ProductName/Version' (e.g. 'MyApp/1.0').");
		}

		UserAgent = userAgent;

		JsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;

		AddCoreCustomFieldValueSerializers();
		AddDefaultJsonConverters();
	}

	/// <summary>
	/// Create a new instance of the settings with default values.
	/// </summary>
	/// <remarks>
	/// This constructor uses a default User-Agent string. Provide an explicit User-Agent
	/// via <see cref="JiraRestClientSettings(string)"/> to identify your application.
	/// </remarks>
	[Obsolete($"Use the constructor that accepts a userAgent parameter to identify your application (e.g. new JiraRestClientSettings(\"MyApp/1.0\")).")]
	public JiraRestClientSettings()
		: this(DefaultUserAgent)
	{
	}

	private void UpdateSerializers()
	{
		AddCoreCustomFieldValueSerializers();
		RemoveKnownJsonConverters();

		if (EnableUserPrivacyMode)
		{
			AddGdprCustomFieldValueSerializers();
			AddGdprJsonConverters();
		}
		else
		{
			AddDefaultJsonConverters();
		}
	}

	private void AddGdprJsonConverters()
	{
		foreach (var converter in _gdprJsonConverters)
		{
			JsonSerializerSettings.Converters.Add(converter);
		}
	}

	private void AddDefaultJsonConverters()
	{
		foreach (var converter in _defaultJsonConverters)
		{
			JsonSerializerSettings.Converters.Add(converter);
		}
	}

	private void RemoveKnownJsonConverters()
	{
		foreach (var converter in _gdprJsonConverters)
		{
			JsonSerializerSettings.Converters.Remove(converter);
		}

		foreach (var converter in _defaultJsonConverters)
		{
			JsonSerializerSettings.Converters.Remove(converter);
		}
	}

	private void AddGdprCustomFieldValueSerializers()
	{
		CustomFieldSerializers[GetBuiltInType("userpicker")] = new SingleObjectCustomFieldValueSerializer("accountId");
		CustomFieldSerializers[GetBuiltInType("multiuserpicker")] = new MultiObjectCustomFieldValueSerializer("accountId");
	}

	private void AddCoreCustomFieldValueSerializers()
	{
		CustomFieldSerializers[GetBuiltInType("labels")] = new MultiStringCustomFieldValueSerializer();
		CustomFieldSerializers[GetBuiltInType("float")] = new FloatCustomFieldValueSerializer();

		CustomFieldSerializers[GetBuiltInType("userpicker")] = new SingleObjectCustomFieldValueSerializer("name");
		CustomFieldSerializers[GetBuiltInType("grouppicker")] = new SingleObjectCustomFieldValueSerializer("name");
		CustomFieldSerializers[GetBuiltInType("project")] = new SingleObjectCustomFieldValueSerializer("key");
		CustomFieldSerializers[GetBuiltInType("radiobuttons")] = new SingleObjectCustomFieldValueSerializer("value");
		CustomFieldSerializers[GetBuiltInType("select")] = new SingleObjectCustomFieldValueSerializer("value");
		CustomFieldSerializers[GetBuiltInType("version")] = new SingleObjectCustomFieldValueSerializer("name");

		CustomFieldSerializers[GetBuiltInType("multigrouppicker")] = new MultiObjectCustomFieldValueSerializer("name");
		CustomFieldSerializers[GetBuiltInType("multiuserpicker")] = new MultiObjectCustomFieldValueSerializer("name");
		CustomFieldSerializers[GetBuiltInType("multiselect")] = new MultiObjectCustomFieldValueSerializer("value");
		CustomFieldSerializers[GetBuiltInType("multiversion")] = new MultiObjectCustomFieldValueSerializer("name");
		CustomFieldSerializers[GetBuiltInType("multicheckboxes")] = new MultiObjectCustomFieldValueSerializer("value");

		CustomFieldSerializers[GetBuiltInType("cascadingselect")] = new CascadingSelectCustomFieldValueSerializer();

		CustomFieldSerializers[GetGreenhopperType("gh-sprint")] = new GreenhopperSprintCustomFieldValueSerialiser("name");
	}

	private static string GetBuiltInType(string name)
	{
		return $"com.atlassian.jira.plugin.system.customfieldtypes:{name}";
	}

	private static string GetGreenhopperType(string name)
	{
		return $"com.pyxis.greenhopper.jira:{name}";
	}
}
