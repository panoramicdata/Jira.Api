using Newtonsoft.Json;

namespace Jira.Api;

/// <summary>
/// Time tracking information for an issue.
/// </summary>
/// <remarks>
/// Creates a new instance of the IssueTimeTrackingData class.
/// </remarks>
public class IssueTimeTrackingData(string originalEstimate, string? remainingEstimate = null)
{
	/// <summary>
	/// The original estimate in human-readable format (e.g., "2h 30m")
	/// </summary>
	[JsonProperty("originalEstimate")]
	public string OriginalEstimate { get; private set; } = originalEstimate;

	/// <summary>
	/// The original estimate in seconds
	/// </summary>
	[JsonProperty("originalEstimateSeconds")]
	public long? OriginalEstimateInSeconds { get; private set; }

	/// <summary>
	/// The remaining estimate in human-readable format (e.g., "1h 15m")
	/// </summary>
	[JsonProperty("remainingEstimate")]
	public string? RemainingEstimate { get; private set; } = remainingEstimate;

	/// <summary>
	/// The remaining estimate in seconds
	/// </summary>
	[JsonProperty("remainingEstimateSeconds")]
	public long? RemainingEstimateInSeconds { get; private set; }

	/// <summary>
	/// The time spent in human-readable format (e.g., "1h 15m")
	/// </summary>
	[JsonProperty("timeSpent")]
	public string TimeSpent { get; private set; }

	/// <summary>
	/// The time spent in seconds
	/// </summary>
	[JsonProperty("timeSpentSeconds")]
	public long? TimeSpentInSeconds { get; private set; }
}
