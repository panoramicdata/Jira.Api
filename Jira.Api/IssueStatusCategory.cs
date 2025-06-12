using Jira.Api.Remote;

namespace Jira.Api;

/// <summary>
/// The category of an issue status as defined in JIRA.
/// </summary>
/// <remarks>
/// Creates an instance of the IssueStatusCategory based on a remote entity.
/// </remarks>
public class IssueStatusCategory(RemoteStatusCategory remoteStatusCategory) : JiraNamedEntity(remoteStatusCategory)
{
	private readonly RemoteStatusCategory _remoteStatusCategory = remoteStatusCategory;

	/// <summary>
	/// The color assigned to this category.
	/// </summary>
	public string ColorName
	{
		get
		{
			return _remoteStatusCategory?.ColorName;
		}
	}

	/// <summary>
	/// The key assigned to this category.
	/// </summary>
	public string Key
	{
		get
		{
			return _remoteStatusCategory?.Key;
		}
	}
}
