using Jira.Api.Remote;
using System.Collections.Generic;
using System.Linq;

namespace Jira.Api;

/// <summary>
/// An issue transition as defined in JIRA.
/// </summary>
public class IssueTransition : JiraNamedEntity
{
	/// <summary>
	/// Creates an instance of the IssueTransition based on a remote entity.
	/// </summary>
	public IssueTransition(RemoteTransition remoteEntity)
		: base(remoteEntity)
	{
		To = remoteEntity.to == null ? null : new IssueStatus(remoteEntity.to);
		HasScreen = remoteEntity.hasScreen;
		IsGlobal = remoteEntity.isGlobal;
		IsInitial = remoteEntity.isInitial;
		IsConditional = remoteEntity.isConditional;
		Fields = remoteEntity.fields?.ToDictionary(x => x.Key, x => new IssueFieldEditMetadata(x.Value));
	}

	/// <summary>
	/// Creates an instance of the IssueTransition with the given id and name.
	/// </summary>
	public IssueTransition(string id, string? name = null)
		: base(id, name)
	{
	}

	/// <summary>
	/// The status this transition leads to
	/// </summary>
	public IssueStatus To { get; private set; }

	/// <summary>
	/// Whether this transition has a screen
	/// </summary>
	public bool HasScreen { get; private set; }

	/// <summary>
	/// Whether this is a global transition
	/// </summary>
	public bool IsGlobal { get; private set; }

	/// <summary>
	/// Whether this is an initial transition
	/// </summary>
	public bool IsInitial { get; private set; }

	/// <summary>
	/// Whether this transition is conditional
	/// </summary>
	public bool IsConditional { get; private set; }

	/// <summary>
	/// The fields and their metadata for this transition
	/// </summary>
	public Dictionary<string, IssueFieldEditMetadata> Fields { get; private set; }
}
