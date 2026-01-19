using System.Collections.ObjectModel;

namespace Jira.Api.Models;

/// <summary>
/// A collection of JIRA named entities
/// </summary>
/// <typeparam name="T">The type of named entity</typeparam>
public class JiraNamedEntityCollection<T> : Collection<T>, IRemoteIssueFieldProvider where T : JiraNamedEntity
{
	/// <summary>
	/// The JIRA client
	/// </summary>
	protected readonly JiraClient _jira;

	/// <summary>
	/// The project key
	/// </summary>
	protected readonly string _projectKey;

	/// <summary>
	/// The field name
	/// </summary>
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

	/// <summary>
	/// Equality operator for checking if an entity with the given name exists in the collection
	/// </summary>
	public static bool operator ==(JiraNamedEntityCollection<T> list, string value)
	{
		return list is null ? value == null : list.Any(v => v.Name == value);
	}

	/// <summary>
	/// Inequality operator for checking if an entity with the given name does not exist in the collection
	/// </summary>
	public static bool operator !=(JiraNamedEntityCollection<T> list, string value)
	{
		return list is null ? value != null : !list.Any(v => v.Name == value);
	}

	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is JiraNamedEntityCollection<T> other && this.SequenceEqual(other);

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		var hashCode = 0;
		foreach (var item in this)
		{
			hashCode ^= item?.GetHashCode() ?? 0;
		}

		return hashCode;
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
