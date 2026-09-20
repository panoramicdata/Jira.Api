using Newtonsoft.Json;
using System.Globalization;

namespace Jira.Api.Models;

/// <summary>
/// Represents a gadget on a JIRA dashboard, as returned by the JIRA Server
/// internal REST API (GET rest/dashboards/1.0/{id}).
/// </summary>
public class JiraDashboardGadget
{
	/// <summary>
	/// The AMD module name of the Filter Results gadget.
	/// </summary>
	public const string FilterResultsAmdModule = "jira-dashboard-items/filter-results";

	private const string FilterIdUserPrefName = "filterId";
	private const string ColumnNamesUserPrefName = "columnNames";
	private const string NumberToShowUserPrefName = "num";
	private const string RefreshUserPrefName = "refresh";

	/// <summary>
	/// Identifier of this gadget.
	/// </summary>
	[JsonProperty("id")]
	public long Id { get; private set; }

	/// <summary>
	/// Title of this gadget. Note that for Filter Results gadgets this is the generic
	/// gadget name (e.g. "Filter Results"); the JIRA web UI appends the filter name,
	/// which must be resolved separately via the filter identified by <see cref="FilterId"/>.
	/// </summary>
	[JsonProperty("title")]
	public string? Title { get; private set; }

	/// <summary>
	/// Zero-based index of the dashboard column containing this gadget.
	/// </summary>
	[JsonProperty("column")]
	public int Column { get; private set; }

	/// <summary>
	/// Color code of this gadget's chrome (e.g. "color1").
	/// </summary>
	[JsonProperty("color")]
	public string? Color { get; private set; }

	/// <summary>
	/// Url of this gadget's specification, where applicable.
	/// </summary>
	[JsonProperty("gadgetUrl")]
	public string? GadgetUrl { get; private set; }

	/// <summary>
	/// AMD module of this gadget, where applicable (e.g. "jira-dashboard-items/filter-results").
	/// </summary>
	[JsonProperty("amdModule")]
	public string? AmdModule { get; private set; }

	/// <summary>
	/// The user preferences configured for this gadget.
	/// </summary>
	[JsonProperty("userPrefs")]
	public JiraDashboardGadgetUserPrefs? UserPrefs { get; private set; }

	/// <summary>
	/// Whether this gadget is a Filter Results gadget (a table of issues backed by a saved filter).
	/// </summary>
	[JsonIgnore]
	public bool IsFilterResults =>
		AmdModule == FilterResultsAmdModule
		|| GadgetUrl?.Contains("filter-results-gadget", StringComparison.OrdinalIgnoreCase) == true;

	/// <summary>
	/// The identifier of the saved filter backing this gadget, from the "filterId" user
	/// preference. Null when not configured (e.g. for non-Filter Results gadgets).
	/// </summary>
	[JsonIgnore]
	public string? FilterId => GetUserPrefValue(FilterIdUserPrefName);

	/// <summary>
	/// The issue table columns configured for this gadget, from the pipe-separated
	/// "columnNames" user preference (e.g. "priority|issuekey|summary|assignee|status|updated").
	/// Empty when not configured.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<string> ColumnNames =>
		GetUserPrefValue(ColumnNamesUserPrefName)
			?.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			?? [];

	/// <summary>
	/// The number of issues this gadget displays per page, from the "num" user preference.
	/// Null when not configured or unparseable.
	/// </summary>
	[JsonIgnore]
	public int? NumberToShow => ParseUserPrefInt(NumberToShowUserPrefName);

	/// <summary>
	/// The auto-refresh interval in minutes, from the "refresh" user preference.
	/// Null when not configured or unparseable.
	/// </summary>
	[JsonIgnore]
	public int? RefreshMinutes => ParseUserPrefInt(RefreshUserPrefName);

	/// <summary>
	/// Returns the value of the user preference with the given name, or null if absent.
	/// </summary>
	/// <param name="name">Name of the user preference.</param>
	public string? GetUserPrefValue(string name) =>
		UserPrefs?.Fields?.FirstOrDefault(field => field.Name == name)?.Value;

	private int? ParseUserPrefInt(string name) =>
		int.TryParse(GetUserPrefValue(name), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
			? value
			: null;
}
