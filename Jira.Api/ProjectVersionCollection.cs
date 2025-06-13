using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api;

/// <summary>
/// Collection of project versions
/// </summary>
public class ProjectVersionCollection : JiraNamedEntityCollection<ProjectVersion>
{
	internal ProjectVersionCollection(string fieldName, Jira jira, string projectKey)
		: this(fieldName, jira, projectKey, [])
	{
	}

	internal ProjectVersionCollection(string fieldName, Jira jira, string projectKey, IList<ProjectVersion> list)
		: base(fieldName, jira, projectKey, list)
	{
	}

	/// <summary>
	/// Add a version by name
	/// </summary>
	/// <param name="versionName">Version name</param>
	public async Task AddAsync(string versionName, CancellationToken cancellationToken)
	{
		var version = (await _jira.Versions.GetVersionsAsync(_projectKey, cancellationToken)).FirstOrDefault(v => v.Name.Equals(versionName, StringComparison.OrdinalIgnoreCase)) ?? throw new InvalidOperationException($"Unable to find version with name '{versionName}'.");
		Add(version);
	}
}
