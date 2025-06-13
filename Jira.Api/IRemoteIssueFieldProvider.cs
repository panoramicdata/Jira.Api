using Jira.Api.Remote;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api;

/// <summary>
/// Represents a type that can provide RemoteFieldValues.
/// </summary>
public interface IRemoteIssueFieldProvider
{
	Task<RemoteFieldValue[]> GetRemoteFieldValuesAsync(CancellationToken cancellationToken);
}
