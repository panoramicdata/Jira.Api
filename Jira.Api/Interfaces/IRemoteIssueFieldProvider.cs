namespace Jira.Api.Interfaces;

/// <summary>
/// Represents a type that can provide RemoteFieldValues.
/// </summary>
public interface IRemoteIssueFieldProvider
{
	/// <summary>
	/// Retrieves the remote field values.
	/// </summary>
	Task<RemoteFieldValue[]> GetRemoteFieldValuesAsync(CancellationToken cancellationToken = default);
}
