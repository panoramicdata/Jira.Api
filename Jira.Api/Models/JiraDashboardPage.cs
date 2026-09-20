using Newtonsoft.Json;

namespace Jira.Api.Models;

/// <summary>
/// Represents a page of JIRA dashboards as returned by the public REST API
/// (GET rest/api/2/dashboard).
/// </summary>
public class JiraDashboardPage
{
	/// <summary>
	/// Index of the first dashboard in this page.
	/// </summary>
	[JsonProperty("startAt")]
	public int StartAt { get; private set; }

	/// <summary>
	/// Maximum number of dashboards per page.
	/// </summary>
	[JsonProperty("maxResults")]
	public int MaxResults { get; private set; }

	/// <summary>
	/// Total number of dashboards available.
	/// </summary>
	[JsonProperty("total")]
	public int Total { get; private set; }

	/// <summary>
	/// The dashboards in this page.
	/// </summary>
	[JsonProperty("dashboards")]
	public List<JiraDashboard> Dashboards { get; private set; } = [];
}
