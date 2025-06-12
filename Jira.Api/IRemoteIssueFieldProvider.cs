using System.Threading;
using System.Threading.Tasks;
using Jira.Api.Remote;

namespace Jira.Api;

/// <summary>
/// Represents a type that can provide RemoteFieldValues.
/// </summary>
public interface IRemoteIssueFieldProvider
{
	Task<RemoteFieldValue[]> GetRemoteFieldValuesAsync(CancellationToken token);
}
