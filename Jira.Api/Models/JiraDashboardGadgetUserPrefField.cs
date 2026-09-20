using Newtonsoft.Json;

namespace Jira.Api.Models;

/// <summary>
/// Represents a single user preference field of a JIRA dashboard gadget.
/// </summary>
public class JiraDashboardGadgetUserPrefField
{
	/// <summary>
	/// Name of this user preference (e.g. "filterId", "columnNames", "num").
	/// </summary>
	[JsonProperty("name")]
	public string? Name { get; private set; }

	/// <summary>
	/// Value of this user preference.
	/// </summary>
	[JsonProperty("value")]
	public string? Value { get; private set; }
}
