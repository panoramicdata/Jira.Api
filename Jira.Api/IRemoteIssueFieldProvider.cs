using Jira.Api.Remote;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api;

/// <summary>
/// Represents a type that can provide RemoteFieldValues.
/// </summary>
public interface IRemoteIssueFieldProvider
{
	/// <summary>
	/// Retrieves the remote field values
	/// </summary>
	/// <param name="cancellationToken">A cancellation token</param>
	/// <returns>An array of remote field values</returns>
	Task<RemoteFieldValue[]> GetRemoteFieldValuesAsync(CancellationToken cancellationToken);
}
