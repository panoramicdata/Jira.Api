using System;

namespace Jira.Api;

/// <summary>
/// Status of a JIRA user
/// </summary>
[Flags]
public enum JiraUserStatus
{
	/// <summary>
	/// User is active
	/// </summary>
	Active = 1,

	/// <summary>
	/// User is inactive
	/// </summary>
	Inactive = 2
}
