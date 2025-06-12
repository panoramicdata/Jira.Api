using System;
using Jira.Api.Remote;

namespace Jira.Api;

/// <summary>
/// Represents the worklog of an issue
/// </summary>
public class Worklog
{
	public string Author { get; set; }
	public JiraUser AuthorUser { get; private set; }
	public string Comment { get; set; }
	public DateTime? StartDate { get; set; }
	public string TimeSpent { get; set; }

	public string Id { get; private set; }

	public long TimeSpentInSeconds { get; private set; }

	public DateTime? CreateDate { get; private set; }

	public DateTime? UpdateDate { get; private set; }

	/// <summary>
	/// Creates a new worklog instance
	/// </summary>
	/// <param name="timeSpent">Specifies a time duration in JIRA duration format, representing the time spent working</param>
	/// <param name="startDate">When the work was started</param>
	/// <param name="comment">An optional comment to describe the work</param>
	public Worklog(string timeSpent, DateTime startDate, string comment = null)
	{
		TimeSpent = timeSpent;
		StartDate = startDate;
		Comment = comment;
	}

	internal Worklog(RemoteWorklog remoteWorklog)
	{
		if (remoteWorklog != null)
		{
			Author = remoteWorklog.authorUser?.InternalIdentifier;
			AuthorUser = remoteWorklog.authorUser;
			Comment = remoteWorklog.comment;
			StartDate = remoteWorklog.startDate;
			TimeSpent = remoteWorklog.timeSpent;
			Id = remoteWorklog.id;
			CreateDate = remoteWorklog.created;
			TimeSpentInSeconds = remoteWorklog.timeSpentInSeconds;
			UpdateDate = remoteWorklog.updated;
		}
	}

	internal RemoteWorklog ToRemote()
	{
		return new RemoteWorklog()
		{
			authorUser = Author == null ? null : new JiraUser() { InternalIdentifier = Author },
			comment = Comment,
			startDate = StartDate,
			timeSpent = TimeSpent
		};
	}
}
