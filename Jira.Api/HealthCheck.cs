using Jira.Api.Remote;

namespace Jira.Api;

/// <summary>
/// The server info health check.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="HealthCheck"/> class.
/// </remarks>
/// <param name="remoteHealthCheck">The remote health check.</param>
public class HealthCheck(RemoteHealthCheck remoteHealthCheck)
{

	/// <summary>
	/// Gets the name.
	/// </summary>
	public string Name { get; } = remoteHealthCheck.name;

	/// <summary>
	/// Gets the description.
	/// </summary>
	public string Description { get; } = remoteHealthCheck.description;

	/// <summary>
	/// Gets a value indicating whether this <see cref="HealthCheck"/> is passed.
	/// </summary>
	/// <value>
	///   <c>true</c> if passed; otherwise, <c>false</c>.
	/// </value>
	public bool Passed { get; } = remoteHealthCheck.passed;
}
