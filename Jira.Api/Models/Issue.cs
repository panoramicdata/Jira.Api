using Jira.Api.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;

namespace Jira.Api.Models;

/// <summary>
/// A JIRA issue
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2360:Optional parameters should not be used", Justification = "Published public API: replacing optional parameters with overloads is a breaking change for existing consumers.")]
public partial class Issue : IRemoteIssueFieldProvider
{
	/// <summary>
	/// Create a new Issue.
	/// </summary>
	/// <param name="jira">Jira instance that owns this issue.</param>
	/// <param name="fields">Fields to be included in the payload when creating the issue.</param>
	public Issue(JiraClient jira, CreateIssueFields fields)
		: this(jira, new RemoteIssue { project = fields.ProjectKey, timeTracking = fields.TimeTrackingData }, fields.ParentIssueKey)
	{
	}

	/// <summary>
	/// Creates a new Issue.
	/// </summary>
	/// <param name="jira">Jira instance that owns this issue.</param>
	/// <param name="projectKey">Project key that owns this issue.</param>
	/// <param name="parentIssueKey">If provided, marks this issue as a subtask of the given parent issue.</param>
	public Issue(JiraClient jira, string projectKey, string? parentIssueKey = null)
		: this(jira, new RemoteIssue { project = projectKey }, parentIssueKey)
	{
	}

	/// <summary>
	/// Creates a new Issue from a remote issue.
	/// </summary>
	/// <param name="jira">The Jira instance that owns this issue.</param>
	/// <param name="remoteIssue">The remote issue object.</param>
	/// <param name="parentIssueKey">If provided, marks this issue as a subtask of the given parent issue.</param>
	public Issue(JiraClient jira, RemoteIssue remoteIssue, string? parentIssueKey = null)
	{
		Jira = jira;
		ParentIssueKey = parentIssueKey;
		Initialize(remoteIssue);
	}

	private void Initialize(RemoteIssue remoteIssue)
	{
		OriginalRemoteIssue = remoteIssue;

		InitializeScalarFields(remoteIssue);
		InitializeNamedEntities(remoteIssue);
		InitializeCollections();

		// additional fields
		AdditionalFields = new IssueFields(OriginalRemoteIssue, Jira);
	}

	private void InitializeScalarFields(RemoteIssue remoteIssue)
	{
		Project = remoteIssue.project;
		Key = remoteIssue.key;
		Created = remoteIssue.created;
		DueDate = remoteIssue.duedate;
		Updated = remoteIssue.updated;
		ResolutionDate = remoteIssue.resolutionDateReadOnly;
		SecurityLevel = remoteIssue.securityLevelReadOnly;

		TimeTrackingData = remoteIssue.timeTracking;
		Assignee = remoteIssue.assignee;
		AssigneeUser = remoteIssue.assigneeJiraUser;
		Description = remoteIssue.description;
		Environment = remoteIssue.environment;
		Reporter = remoteIssue.reporter;
		ReporterUser = remoteIssue.reporterJiraUser;
		Summary = remoteIssue.summary;
		Votes = remoteIssue.votesData?.votes;
		HasUserVoted = remoteIssue.votesData != null && remoteIssue.votesData.hasVoted;

		if (!string.IsNullOrEmpty(remoteIssue.parentKey))
		{
			ParentIssueKey = remoteIssue.parentKey;
		}
	}

	private static IssueStatus? CreateIssueStatus(RemoteStatus? status)
		=> status == null ? null : new IssueStatus(status);

	private static IssuePriority? CreateIssuePriority(RemotePriority? priority)
		=> priority == null ? null : new IssuePriority(priority);

	private static IssueResolution? CreateIssueResolution(RemoteResolution? resolution)
		=> resolution == null ? null : new IssueResolution(resolution);

	private static IssueType? CreateIssueType(RemoteIssueType? type)
		=> type == null ? null : new IssueType(type);
}
