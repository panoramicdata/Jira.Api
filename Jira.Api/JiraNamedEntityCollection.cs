using Jira.Api.Remote;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api;

[SuppressMessage("N/A", "CS0660", Justification = "Operator overloads are used for LINQ to JQL provider.")]
[SuppressMessage("N/A", "CS0661", Justification = "Operator overloads are used for LINQ to JQL provider.")]
public class JiraNamedEntityCollection<T> : Collection<T>, IRemoteIssueFieldProvider where T : JiraNamedEntity
{
	protected readonly JiraClient _jira;
	protected readonly string _projectKey;
	protected readonly string _fieldName;
	private readonly List<T> _originalList;

	internal JiraNamedEntityCollection(string fieldName, JiraClient jira, string projectKey, IList<T> list)
		: base(list)
	{
		_fieldName = fieldName;
		_jira = jira;
		_projectKey = projectKey;
		_originalList = [.. list];
	}

	public static bool operator ==(JiraNamedEntityCollection<T> list, string value)
	{
		return list is null ? value == null : list.Any(v => v.Name == value);
	}

	public static bool operator !=(JiraNamedEntityCollection<T> list, string value)
	{
		return list is null ? value == null : !list.Any(v => v.Name == value);
	}

	/// <summary>
	/// Removes an entity by name.
	/// </summary>
	/// <param name="name">Entity name.</param>
	public void Remove(string name)
	{
		Remove(Items.First(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));
	}

	Task<RemoteFieldValue[]> IRemoteIssueFieldProvider.GetRemoteFieldValuesAsync(CancellationToken cancellationToken)
	{
		var fields = new List<RemoteFieldValue>();

		if (_originalList.Count != Items.Count || _originalList.Except(Items).Any())
		{
			var field = new RemoteFieldValue()
			{
				id = _fieldName,
				values = [.. Items.Select(e => e.Id)]
			};
			fields.Add(field);
		}

		return Task.FromResult(fields.ToArray());
	}
}
