namespace Jira.Api.Models;

/// <summary>
/// The status of the issue as defined in JIRA
/// </summary>
public class IssueStatus : JiraNamedConstant
{
	/// <summary>
	/// Creates an instance of the IssueStatus based on a remote entity.
	/// </summary>
	public IssueStatus(RemoteStatus remoteStatus)
		: base(remoteStatus)
	{
		StatusCategory = remoteStatus.statusCategory != null ?
			new IssueStatusCategory(remoteStatus.statusCategory) :
			null!;
	}

	internal IssueStatus(string id, string? name = null)
		: base(id, name)
	{
		StatusCategory = null!;
	}

	/// <summary>
	/// Retrieves the list of statuses from JIRA
	/// </summary>
	protected override async Task<IEnumerable<JiraNamedEntity>> GetEntitiesAsync(JiraClient jira, CancellationToken cancellationToken)
	{
		var results = await jira.Statuses.GetStatusesAsync(cancellationToken).ConfigureAwait(false);
		return results as IEnumerable<JiraNamedEntity>;
	}

	/// <summary>
	/// The category assigned to this issue status.
	/// </summary>
	public IssueStatusCategory StatusCategory { get; }

	/// <summary>
	/// Allows assignation by name
	/// </summary>
	public static implicit operator IssueStatus(string name)
	{
		if (name != null)
		{
			if (int.TryParse(name, out int id))
			{
				return new IssueStatus(name /*as id*/);
			}
			else
			{
				return new IssueStatus(null!, name);
			}
		}
		else
		{
			return null!;
		}
	}

	/// <summary>
	/// Operator overload to simplify LINQ queries
	/// </summary>
	/// <remarks>
	/// Allows calls in the form of issue.Priority == "High"
	/// </remarks>
	public static bool operator ==(IssueStatus? entity, string? name)
	{
		if (entity is null)
		{
			return name == null;
		}
		else if (name == null)
		{
			return false;
		}
		else
		{
			return entity.Name == name;
		}
	}

	/// <summary>
	/// Operator overload to simplify LINQ queries
	/// </summary>
	/// <remarks>
	/// Allows calls in the form of issue.Priority != "High"
	/// </remarks>
	public static bool operator !=(IssueStatus? entity, string? name)
	{
		if (entity is null)
		{
			return name != null;
		}
		else if (name == null)
		{
			return true;
		}
		else
		{
			return entity.Name != name;
		}
	}

	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is IssueStatus other && Name == other.Name;

	/// <inheritdoc/>
	public override int GetHashCode() => Name?.GetHashCode() ?? 0;
}
