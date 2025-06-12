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

	public string Id { get; private set; } = remoteComment.id;

	public string Author { get; set; } = remoteComment.authorUser?.InternalIdentifier;

	public JiraUser AuthorUser { get; private set; } = remoteComment.authorUser;

	public string Body { get; set; } = remoteComment.body;

	public string GroupLevel { get; set; } = remoteComment.groupLevel;

	public string RoleLevel { get; set; } = remoteComment.roleLevel;

	public DateTime? CreatedDate { get; private set; } = remoteComment.created;

	public string UpdateAuthor { get; private set; } = remoteComment.updateAuthorUser?.InternalIdentifier;

	public JiraUser UpdateAuthorUser { get; private set; } = remoteComment.updateAuthorUser;

	public DateTime? UpdatedDate { get; private set; } = remoteComment.updated;

	public CommentVisibility Visibility { get; set; } = remoteComment.visibility;

	public string RenderedBody { get; set; } = remoteComment.renderedBody;

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
			authorUser = Author == null ? null : new JiraUser() { InternalIdentifier = Author },
			body = Body,
			groupLevel = GroupLevel,
			roleLevel = RoleLevel,
			visibility = Visibility
		};
	}
}
