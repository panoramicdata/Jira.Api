using System;
using Jira.Api.Remote;
using System.Collections.Generic;
using System.Linq;

namespace Jira.Api;

/// <summary>
/// A comment associated with an issue
/// </summary>
/// <remarks>
/// Create a new Comment from a remote instance object.
/// </remarks>
/// <param name="remoteComment">The remote comment.</param>
public class Comment(RemoteComment remoteComment)
{
	private readonly IEnumerable<RemoteCommentProperty> _properties = remoteComment.properties;
	private Dictionary<string, object> _propertiesMap;

	/// <summary>
	/// Create a new Comment.
	/// </summary>
	public Comment() :
		this(new RemoteComment())
	{
	}

	/// <summary>
	/// The comment identifier
	/// </summary>
	public string Id { get; private set; } = remoteComment.id;

	/// <summary>
	/// The author identifier
	/// </summary>
	public string Author { get; set; } = remoteComment.authorUser?.InternalIdentifier;

	/// <summary>
	/// The author user details
	/// </summary>
	public JiraUser AuthorUser { get; private set; } = remoteComment.authorUser;

	/// <summary>
	/// The comment body text
	/// </summary>
	public string Body { get; set; } = remoteComment.body;

	/// <summary>
	/// The group level for comment visibility
	/// </summary>
	public string GroupLevel { get; set; } = remoteComment.groupLevel;

	/// <summary>
	/// The role level for comment visibility
	/// </summary>
	public string RoleLevel { get; set; } = remoteComment.roleLevel;

	/// <summary>
	/// The date and time when the comment was created
	/// </summary>
	public DateTime? CreatedDate { get; private set; } = remoteComment.created;

	/// <summary>
	/// The identifier of the user who last updated the comment
	/// </summary>
	public string UpdateAuthor { get; private set; } = remoteComment.updateAuthorUser?.InternalIdentifier;

	/// <summary>
	/// The user details of who last updated the comment
	/// </summary>
	public JiraUser UpdateAuthorUser { get; private set; } = remoteComment.updateAuthorUser;

	/// <summary>
	/// The date and time when the comment was last updated
	/// </summary>
	public DateTime? UpdatedDate { get; private set; } = remoteComment.updated;

	/// <summary>
	/// The visibility settings for the comment
	/// </summary>
	public CommentVisibility Visibility { get; set; } = remoteComment.visibility;

	/// <summary>
	/// The rendered HTML body of the comment
	/// </summary>
	public string RenderedBody { get; set; } = remoteComment.renderedBody;

	/// <summary>
	/// The custom properties associated with the comment
	/// </summary>
	public IReadOnlyDictionary<string, object> Properties
	{
		get
		{
			if (_propertiesMap == null)
			{
				if (_properties == null)
				{
					_propertiesMap = [];
				}
				else
				{
					_propertiesMap = _properties.ToDictionary(prop => prop.key, prop => prop.value);
				}
			}

			return _propertiesMap;
		}

	}

	internal RemoteComment ToRemote()
	{
		return new RemoteComment
		{
			authorUser = Author == null ? null : new JiraUser { InternalIdentifier = Author },
			body = Body,
			groupLevel = GroupLevel,
			roleLevel = RoleLevel,
			visibility = Visibility
		};
	}
}
