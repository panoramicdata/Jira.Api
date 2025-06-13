using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api;

/// <summary>
/// Collection of project components
/// </summary>
public class ProjectComponentCollection : JiraNamedEntityCollection<ProjectComponent>
{
	internal ProjectComponentCollection(string fieldName, Jira jira, string projectKey)
		: this(fieldName, jira, projectKey, [])
	{
	}

	internal ProjectComponentCollection(string fieldName, Jira jira, string projectKey, IList<ProjectComponent> list)
		: base(fieldName, jira, projectKey, list)
	{
	}

	/// <summary>
	/// Add a component by name
	/// </summary>
	/// <param name="componentName">Component name</param>
	public async Task AddAsync(string componentName, CancellationToken cancellationToken)
	{
		var component = (await _jira.Components.GetComponentsAsync(_projectKey, cancellationToken)).FirstOrDefault(v => v.Name.Equals(componentName, StringComparison.OrdinalIgnoreCase)) ?? throw new InvalidOperationException($"Unable to find component with name '{componentName}'.");
		Add(component);
	}
}
