using Newtonsoft.Json;

namespace Jira.Api.Models;

/// <summary>
/// Represents the user preferences of a JIRA dashboard gadget.
/// </summary>
public class JiraDashboardGadgetUserPrefs
{
	/// <summary>
	/// Url of the REST resource for modifying these preferences.
	/// </summary>
	[JsonProperty("action")]
	public string? Action { get; private set; }

	/// <summary>
	/// The individual user preference fields.
	/// </summary>
	[JsonProperty("fields")]
	public List<JiraDashboardGadgetUserPrefField> Fields { get; private set; } = [];
}
