using Newtonsoft.Json;

namespace Jira.Api;

/// <summary>
/// Represents a JIRA filter.
/// </summary>
/// <remarks>
/// Creates an instance of a JiraFilter.
/// </remarks>
/// <param name="id">Identifier of the resource.</param>
/// <param name="name">Name of the resource.</param>
/// <param name="jql">Jql of the filter.</param>
/// <param name="self">Url to the resource.</param>
public class JiraFilter(string id, string name, string? jql = null, string? self = null) : JiraNamedResource(id, name, self)
{

	/// <summary>
	/// JQL for this filter.
	/// </summary>
	[JsonProperty("jql")]
	public string Jql { get; private set; } = jql;

	/// <summary>
	/// Description for this filter.
	/// </summary>
	[JsonProperty("description")]
	public string Description { get; private set; }
}
