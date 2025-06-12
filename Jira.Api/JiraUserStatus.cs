using System;

namespace Jira.Api;

[Flags]
public enum JiraUserStatus
{
	Active = 1,
	Inactive = 2
}
