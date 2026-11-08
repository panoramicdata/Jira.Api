namespace Jira.Api.Interfaces;

/// <summary>
/// Represents the operations on the issue priorities of jira.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2360:Optional parameters should not be used", Justification = "Published public API: replacing optional parameters with overloads is a breaking change for existing consumers and implementers.")]
public interface IIssuePriorityService
{
	/// <summary>
	/// Returns all the issue priorities within JIRA.
	/// </summary>
	Task<IEnumerable<IssuePriority>> GetPrioritiesAsync(CancellationToken cancellationToken = default);
}
