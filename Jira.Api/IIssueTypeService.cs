﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api;

/// <summary>
/// Represents the operations on the issue types of jira.
/// Maps to https://docs.atlassian.com/jira/REST/latest/#api/2/issuetype.
/// </summary>
public interface IIssueTypeService
{
	/// <summary>
	/// Returns all the issue types within JIRA.
	/// </summary>
	Task<IEnumerable<IssueType>> GetIssueTypesAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Returns the issue types within JIRA for the project specified.
	/// </summary>
	Task<IEnumerable<IssueType>> GetIssueTypesForProjectAsync(string projectKey, CancellationToken cancellationToken);

}
