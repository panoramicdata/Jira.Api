namespace Jira.Api.Interfaces;

/// <summary>
/// Represents the operations on the issue priorities of jira.
/// </summary>
public interface IIssuePriorityService
{
	/// <summary>
	/// Returns all the issue priorities within JIRA.
	/// </summary>
	Task<IEnumerable<IssuePriority>> GetPrioritiesAsync(CancellationToken cancellationToken = default);
}
