namespace Jira.Api.Interfaces;

/// <summary>
/// Represents the operations on the Jira server info.
/// </summary>
public interface IServerInfoService
{
	/// <summary>
	/// Gets the server information.
	/// </summary>
	Task<ServerInfo> GetServerInfoAsync(bool doHealthCheck = false, CancellationToken cancellationToken = default);
}
