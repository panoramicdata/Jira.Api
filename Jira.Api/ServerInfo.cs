using Jira.Api.Remote;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jira.Api;

/// <summary>
/// The JIRA server info.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ServerInfo"/> class.
/// </remarks>
/// <param name="remoteServerInfo">The remote server information.</param>
public class ServerInfo(RemoteServerInfo remoteServerInfo)
{

	/// <summary>
	/// Gets the base URL.
	/// </summary>
	public string BaseUrl { get; } = remoteServerInfo.baseUrl;

	/// <summary>
	/// Gets the version.
	/// </summary>
	public string Version { get; } = remoteServerInfo.version;

	/// <summary>
	/// Gets the version numbers.
	/// </summary>
	public int[] VersionNumbers { get; } = remoteServerInfo.versionNumbers;

	/// <summary>
	/// Gets the type of the deployment.
	/// </summary>
	public string DeploymentType { get; } = remoteServerInfo.deploymentType;

	/// <summary>
	/// Gets the build number.
	/// </summary>
	public int BuildNumber { get; } = remoteServerInfo.buildNumber;

	/// <summary>
	/// Gets the build date.
	/// </summary>
	public DateTimeOffset? BuildDate { get; } = remoteServerInfo.buildDate;

	/// <summary>
	/// Gets the server time.
	/// </summary>
	public DateTimeOffset? ServerTime { get; } = remoteServerInfo.serverTime;

	/// <summary>
	/// Gets the SCM information.
	/// </summary>
	public string ScmInfo { get; } = remoteServerInfo.scmInfo;

	/// <summary>
	/// Gets the name of the build partner.
	/// </summary>
	public string BuildPartnerName { get; } = remoteServerInfo.buildPartnerName;

	/// <summary>
	/// Gets the server title.
	/// </summary>
	public string ServerTitle { get; } = remoteServerInfo.serverTitle;

	/// <summary>
	/// Gets the health checks.
	/// </summary>
	public IEnumerable<HealthCheck> HealthChecks { get; } = remoteServerInfo.healthChecks?.Select(x => new HealthCheck(x)).ToArray();
}
