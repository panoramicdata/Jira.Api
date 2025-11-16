namespace Jira.Api;

/// <summary>
/// The worklog time remaining strategy
/// </summary>
public enum WorklogStrategy
{
	/// <summary>
	/// Automatically adjust the remaining estimate
	/// </summary>
	AutoAdjustRemainingEstimate,

	/// <summary>
	/// Keep the current remaining estimate unchanged
	/// </summary>
	RetainRemainingEstimate,

	/// <summary>
	/// Set a new remaining estimate
	/// </summary>
	NewRemainingEstimate
}
