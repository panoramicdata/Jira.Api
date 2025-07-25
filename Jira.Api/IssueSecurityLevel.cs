﻿using Newtonsoft.Json;

namespace Jira.Api;

/// <summary>
/// Represents the security level that can be set on an issue.
/// </summary>
public class IssueSecurityLevel : JiraNamedResource
{
	/// <summary>
	/// Description of this security level.
	/// </summary>
	[JsonProperty("description")]
	public string Description { get; private set; }
}
