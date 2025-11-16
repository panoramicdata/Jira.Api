using Jira.Api.Remote;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api;

/// <summary>
/// The priority of the issue as defined in JIRA
/// </summary>
[SuppressMessage("N/A", "CS0660", Justification = "Operator overloads are used for LINQ to JQL provider.")]
[SuppressMessage("N/A", "CS0661", Justification = "Operator overloads are used for LINQ to JQL provider.")]
public class IssuePriority : JiraNamedConstant
{
	/// <summary>
	/// Creates an instance of the IssuePriority based on a remote entity.
	/// </summary>
	public IssuePriority(RemotePriority remoteEntity)
		: base(remoteEntity)
	{
	}

	/// <summary>
	/// Creates an instance of the IssuePriority with the given id and name.
	/// </summary>
	public IssuePriority(string id, string? name = null)
		: base(id, name)
	{
	}

	/// <summary>
	/// Retrieves the list of priorities from JIRA
	/// </summary>
	protected override async Task<IEnumerable<JiraNamedEntity>> GetEntitiesAsync(JiraClient jira, CancellationToken cancellationToken)
	{
		var priorities = await jira.Priorities.GetPrioritiesAsync(cancellationToken).ConfigureAwait(false);
		return priorities as IEnumerable<JiraNamedEntity>;
	}

	/// <summary>
	/// Allows assignation by name
	/// </summary>
	public static implicit operator IssuePriority(string name)
	{
		if (name != null)
		{
			if (int.TryParse(name, out int id))
			{
				return new IssuePriority(name /*as id*/);
			}
			else
			{
				return new IssuePriority(null, name);
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
	public static bool operator ==(IssuePriority entity, string name)
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
	public static bool operator !=(IssuePriority entity, string name)
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

	/// <summary>
	/// Greater than operator (not implemented)
	/// </summary>
	public static bool operator >(IssuePriority field, string value)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Less than operator (not implemented)
	/// </summary>
	public static bool operator <(IssuePriority field, string value)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Less than or equal operator (not implemented)
	/// </summary>
	public static bool operator <=(IssuePriority field, string value)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Greater than or equal operator (not implemented)
	/// </summary>
	public static bool operator >=(IssuePriority field, string value)
	{
		throw new NotImplementedException();
	}
}
