using Newtonsoft.Json;

namespace Jira.Api.Models;

/// <summary>
/// Represents a JIRA dashboard including its gadgets, as returned by the JIRA Server
/// internal REST API (GET rest/dashboards/1.0/{id}).
/// </summary>
/// <remarks>
/// This endpoint is not part of the public JIRA REST API. It is the endpoint the JIRA
/// Server web UI itself uses to render dashboards and has been verified against JIRA
/// Server. It may not be available on JIRA Cloud.
/// </remarks>
public class JiraDashboardDetail
{
	/// <summary>
	/// Identifier of this dashboard.
	/// </summary>
	[JsonProperty("id")]
	public string? Id { get; private set; }

	/// <summary>
	/// Title of this dashboard.
	/// </summary>
	[JsonProperty("title")]
	public string? Title { get; private set; }

	/// <summary>
	/// Whether the current user may modify this dashboard.
	/// </summary>
	[JsonProperty("writable")]
	public bool Writable { get; private set; }

	/// <summary>
	/// Layout code of this dashboard (e.g. "A" for a single column, "AA" for two columns).
	/// </summary>
	[JsonProperty("layout")]
	public string? Layout { get; private set; }

	/// <summary>
	/// The gadgets on this dashboard. Within a column, gadgets appear in display order.
	/// </summary>
	[JsonProperty("gadgets")]
	public List<JiraDashboardGadget> Gadgets { get; private set; } = [];
}
