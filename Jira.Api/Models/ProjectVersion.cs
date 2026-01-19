namespace Jira.Api.Models;

/// <summary>
/// A version associated with a project
/// </summary>
public class ProjectVersion : JiraNamedEntity
{
	private readonly JiraClient _jira;

	/// <summary>
	/// Creates a new instance of a ProjectVersion.
	/// </summary>
	/// <param name="jira">The jira instance.</param>
	/// <param name="remoteVersion">The remote version.</param>
	public ProjectVersion(JiraClient jira, RemoteVersion remoteVersion)
		: base(remoteVersion)
	{
		ArgumentNullException.ThrowIfNull(jira);

		_jira = jira;
		RemoteVersion = remoteVersion;
	}

	internal RemoteVersion RemoteVersion { get; private set; }

	/// <summary>
	/// Gets the project key associated with this version.
	/// </summary>
	public string ProjectKey
	{
		get
		{
			return RemoteVersion.ProjectKey;
		}
	}

	/// <summary>
	/// Whether this version has been archived
	/// </summary>
	public bool IsArchived
	{
		get
		{
			return RemoteVersion.archived;
		}
		set
		{
			RemoteVersion.archived = value;
		}
	}

	/// <summary>
	/// Whether this version has been released
	/// </summary>
	public bool IsReleased
	{
		get
		{
			return RemoteVersion.released;
		}
		set
		{
			RemoteVersion.released = value;
		}
	}

	/// <summary>
	/// The start date for this version
	/// </summary>
	public DateTime? StartDate
	{
		get
		{
			return RemoteVersion.startDate;
		}
		set
		{
			RemoteVersion.startDate = value;
		}
	}

	/// <summary>
	/// The released date for this version (null if not yet released)
	/// </summary>
	public DateTime? ReleasedDate
	{
		get
		{
			return RemoteVersion.releaseDate;
		}
		set
		{
			RemoteVersion.releaseDate = value;
		}
	}

	/// <summary>
	/// The release description for this version (null if not available)
	/// </summary>
	public string Description
	{
		get
		{
			return RemoteVersion.description;
		}
		set
		{
			RemoteVersion.description = value;
		}
	}

	/// <summary>
	/// Save field changes to the server.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public async Task SaveChangesAsync(CancellationToken cancellationToken)
	{
		var version = await _jira.Versions.UpdateVersionAsync(this, cancellationToken).ConfigureAwait(false);
		RemoteVersion = version.RemoteVersion;
	}
}
