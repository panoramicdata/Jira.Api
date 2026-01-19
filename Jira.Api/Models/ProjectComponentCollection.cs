namespace Jira.Api.Models;

/// <summary>
/// Collection of project components
/// </summary>
public class ProjectComponentCollection : JiraNamedEntityCollection<ProjectComponent>
{
	internal ProjectComponentCollection(string fieldName, JiraClient jira, string projectKey)
		: this(fieldName, jira, projectKey, [])
	{
	}

	internal ProjectComponentCollection(string fieldName, JiraClient jira, string projectKey, IList<ProjectComponent> list)
		: base(fieldName, jira, projectKey, list)
	{
	}

	/// <summary>
	/// Add a component by name
	/// </summary>
	/// <param name="componentName">Component name</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public async Task AddAsync(string componentName, CancellationToken cancellationToken)
	{
		var component = (await _jira.Components.GetComponentsAsync(_projectKey, cancellationToken)).FirstOrDefault(v => v.Name.Equals(componentName, StringComparison.OrdinalIgnoreCase)) ?? throw new InvalidOperationException($"Unable to find component with name '{componentName}'.");
		Add(component);
	}
}
