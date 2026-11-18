namespace Jira.Api.Models;

/// <summary>
/// The resolution of the issue as defined in JIRA
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2360:Optional parameters should not be used", Justification = "Published public API: replacing optional parameters with overloads is a breaking change for existing consumers.")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S3875:operator== should not be overloaded on reference types", Justification = "Published public API: these operators are how consumers write LINQ to JQL comparisons, so removing them is a breaking change.")]
public class IssueResolution : JiraNamedEntity
{
	/// <summary>
	/// Creates an instance of the IssueResolution based on a remote entity.
	/// </summary>
	public IssueResolution(AbstractNamedRemoteEntity remoteEntity)
		: base(remoteEntity)
	{
	}

	/// <summary>
	/// Creates an instance of the IssueResolution with the given id and name.
	/// </summary>
	public IssueResolution(string id, string? name = null)
		: base(id, name)
	{
	}

	/// <summary>
	/// Retrieves the list of resolutions from JIRA
	/// </summary>
	protected override async Task<IEnumerable<JiraNamedEntity>> GetEntitiesAsync(JiraClient jira, CancellationToken cancellationToken)
	{
		var results = await jira.Resolutions.GetResolutionsAsync(cancellationToken).ConfigureAwait(false);
		return results as IEnumerable<JiraNamedEntity>;
	}

	/// <summary>
	/// Allows assignation by name
	/// </summary>
	public static implicit operator IssueResolution(string name) => JiraNamedEntity.FromNameOrId(name, (id, n) => new IssueResolution(id, n));

	/// <summary>
	/// Operator overload to simplify LINQ queries
	/// </summary>
	/// <remarks>
	/// Allows calls in the form of issue.Priority == "High"
	/// </remarks>
	public static bool operator ==(IssueResolution? entity, string? name) => JiraNamedEntity.NameMatches(entity, name);

	/// <summary>
	/// Operator overload to simplify LINQ queries
	/// </summary>
	/// <remarks>
	/// Allows calls in the form of issue.Priority != "High"
	/// </remarks>
	public static bool operator !=(IssueResolution? entity, string? name) => !JiraNamedEntity.NameMatches(entity, name);
	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is IssueResolution other && Name == other.Name;

	/// <inheritdoc/>
	public override int GetHashCode() => Name?.GetHashCode() ?? 0;
}
