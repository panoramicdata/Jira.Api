using Jira.Api.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api;

/// <summary>
/// Represents a named entity within JIRA.
/// </summary>
public class JiraNamedEntity : IJiraEntity
{
	/// <summary>
	/// Creates an instance of a JiraNamedEntity base on a remote entity.
	/// </summary>
	public JiraNamedEntity(AbstractNamedRemoteEntity remoteEntity)
		: this(remoteEntity.id, remoteEntity.name)
	{
	}

	/// <summary>
	/// Creates an instance of a JiraNamedEntity.
	/// </summary>
	/// <param name="id">Identifier of the entity.</param>
	/// <param name="name">Name of the entity.</param>
	public JiraNamedEntity(string id, string? name = null)
	{
		if (string.IsNullOrEmpty(id) && string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException($"Named entity should have and id or a name. Id: '{id}'. Name: '{name}'.");
		}

		Id = id;
		Name = name;
	}

	/// <summary>
	/// Id of the entity.
	/// </summary>
	public string Id { get; protected set; }

	/// <summary>
	/// Name of the entity.
	/// </summary>
	public string? Name { get; protected set; }

	protected virtual Task<IEnumerable<JiraNamedEntity>> GetEntitiesAsync(JiraClient jira, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	public override string ToString()
	{
		if (!string.IsNullOrEmpty(Name))
		{
			return Name;
		}
		else
		{
			return Id;
		}
	}

	internal async Task<JiraNamedEntity> LoadIdAndNameAsync(JiraClient jira, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(Id) || string.IsNullOrEmpty(Name))
		{
			var entities = await GetEntitiesAsync(jira, cancellationToken).ConfigureAwait(false);
			var entity = entities.FirstOrDefault(e =>
				(!string.IsNullOrEmpty(Name) && string.Equals(e.Name, Name, StringComparison.OrdinalIgnoreCase)) ||
				(!string.IsNullOrEmpty(Id) && string.Equals(e.Id, Id, StringComparison.OrdinalIgnoreCase))) ?? throw new InvalidOperationException($"Entity with id '{Id}' and name '{Name}' was not found for type '{GetType()}'. Available: [{string.Join(",", entities.Select(s => s.Id + ":" + s.Name).ToArray())}]");
			Id = entity.Id;
			Name = entity.Name;
		}

		return this;
	}
}

internal class JiraEntityNameEqualityComparer : IEqualityComparer<JiraNamedEntity>
{
	public bool Equals(JiraNamedEntity x, JiraNamedEntity y)
	{
		return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
	}

	public int GetHashCode(JiraNamedEntity obj)
	{
		if (obj == null || obj.Name == null)
		{
			return 0;
		}

		return obj.Name.GetHashCode();
	}
}
