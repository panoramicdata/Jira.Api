using Newtonsoft.Json;

namespace Jira.Api.Models;

/// <summary>
/// Represents a JIRA dashboard as returned by the public REST API
/// (GET rest/api/2/dashboard/{id}).
/// </summary>
public class JiraDashboard
{
	/// <summary>
	/// Identifier of this dashboard.
	/// </summary>
	[JsonProperty("id")]
	public string? Id { get; private set; }

	/// <summary>
	/// Name of this dashboard.
	/// </summary>
	[JsonProperty("name")]
	public string? Name { get; private set; }

	/// <summary>
	/// Url to access this dashboard via the REST API.
	/// </summary>
	[JsonProperty("self")]
	public string? Self { get; private set; }

	/// <summary>
	/// Url to view this dashboard in the JIRA web UI.
	/// </summary>
	[JsonProperty("view")]
	public string? View { get; private set; }
}
