using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api;

/// <summary>
/// Represents the operations on the Jira server info.
/// </summary>
public interface IServerInfoService
{
	/// <summary>
	/// Gets the server information.
	/// </summary>
	/// <param name="doHealthCheck">if set to <c>true</c>, do a health check.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>The server information.</returns>
	Task<ServerInfo> GetServerInfoAsync(bool doHealthCheck, CancellationToken cancellationToken);
}
