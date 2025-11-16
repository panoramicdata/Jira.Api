using Jira.Api.Remote;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api;

/// <summary>
/// The resolution of the issue as defined in JIRA
/// </summary>
[SuppressMessage("N/A", "CS0660", Justification = "Operator overloads are used for LINQ to JQL provider.")]
[SuppressMessage("N/A", "CS0661", Justification = "Operator overloads are used for LINQ to JQL provider.")]
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
	public static implicit operator IssueResolution(string name)
	{
		if (name != null)
		{
			if (int.TryParse(name, out int id))
			{
				return new IssueResolution(name /*as id*/);
			}
			else
			{
				return new IssueResolution(null, name);
			}
		}
		else
		{
			return null;
		}
	}

	/// <summary>
	/// Operator overload to simplify LINQ queries
	/// </summary>
	/// <remarks>
	/// Allows calls in the form of issue.Priority == "High"
	/// </remarks>
	public static bool operator ==(IssueResolution entity, string name)
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
	public static bool operator !=(IssueResolution entity, string name)
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
}
