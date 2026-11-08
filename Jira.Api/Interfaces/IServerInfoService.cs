namespace Jira.Api.Interfaces;

/// <summary>
/// Represents the operations on the Jira server info.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2360:Optional parameters should not be used", Justification = "Published public API: replacing optional parameters with overloads is a breaking change for existing consumers and implementers.")]
public interface IServerInfoService
{
	/// <summary>
	/// Gets the server information.
	/// </summary>
	Task<ServerInfo> GetServerInfoAsync(bool doHealthCheck = false, CancellationToken cancellationToken = default);
}
