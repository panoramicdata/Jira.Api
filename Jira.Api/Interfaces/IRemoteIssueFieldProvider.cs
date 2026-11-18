namespace Jira.Api.Interfaces;

/// <summary>
/// Represents a type that can provide RemoteFieldValues.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2360:Optional parameters should not be used", Justification = "Published public API: replacing optional parameters with overloads is a breaking change for existing consumers and implementers.")]
public interface IRemoteIssueFieldProvider
{
	/// <summary>
	/// Retrieves the remote field values.
	/// </summary>
	Task<RemoteFieldValue[]> GetRemoteFieldValuesAsync(CancellationToken cancellationToken = default);
}
