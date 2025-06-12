using Newtonsoft.Json;

namespace Jira.Api;

/// <summary>
/// Time tracking information for an issue.
/// </summary>
/// <remarks>
/// Creates a new instance of the IssueTimeTrackingData class.
/// </remarks>
public class IssueTimeTrackingData(string originalEstimate, string remainingEstimate = null)
{
	[JsonProperty("originalEstimate")]
	public string OriginalEstimate { get; private set; } = originalEstimate;

	[JsonProperty("originalEstimateSeconds")]
	public long? OriginalEstimateInSeconds { get; private set; }

	[JsonProperty("remainingEstimate")]
	public string RemainingEstimate { get; private set; } = remainingEstimate;

	[JsonProperty("remainingEstimateSeconds")]
	public long? RemainingEstimateInSeconds { get; private set; }

	[JsonProperty("timeSpent")]
	public string TimeSpent { get; private set; }

	[JsonProperty("timeSpentSeconds")]
	public long? TimeSpentInSeconds { get; private set; }
}
