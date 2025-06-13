using Jira.Api.Linq;
using Jira.Api.Remote;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api;

/// <summary>
/// A JIRA issue
/// </summary>
public class Issue : IRemoteIssueFieldProvider
{
	private readonly JiraClient _jira;

	private ComparableString _key;
	private string _project;
	private RemoteIssue _originalIssue;
	private DateTime? _createDate;
	private DateTime? _updateDate;
	private DateTime? _dueDate;
	private DateTime? _resolutionDate;
	private IssueSecurityLevel _securityLevel;
	private ProjectVersionCollection? _affectsVersions = null;
	private ProjectVersionCollection? _fixVersions = null;
	private ProjectComponentCollection? _components = null;
	private CustomFieldValueCollection? _customFields = null;
	private IssueLabelCollection? _labels = null;
	private IssueStatus _status;
	private string? _parentIssueKey;
	private IssueType _issueType;

	/// <summary>
	/// Create a new Issue.
	/// </summary>
	/// <param name="jira">Jira instance that owns this issue.</param>
	/// <param name="fields">Fields to be included in the payload when creating the issue.</param>
	public Issue(JiraClient jira, CreateIssueFields fields)
		: this(jira, new RemoteIssue() { project = fields.ProjectKey, timeTracking = fields.TimeTrackingData }, fields.ParentIssueKey)
	{
	}

	/// <summary>
	/// Creates a new Issue.
	/// </summary>
	/// <param name="jira">Jira instance that owns this issue.</param>
	/// <param name="projectKey">Project key that owns this issue.</param>
	/// <param name="parentIssueKey">If provided, marks this issue as a subtask of the given parent issue.</param>
	public Issue(JiraClient jira, string projectKey, string? parentIssueKey = null)
		: this(jira, new RemoteIssue() { project = projectKey }, parentIssueKey)
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
		_jira = jira;
		_parentIssueKey = parentIssueKey;
		Initialize(remoteIssue);
	}

	private void Initialize(RemoteIssue remoteIssue)
	{
		_originalIssue = remoteIssue;

		_project = remoteIssue.project;
		_key = remoteIssue.key;
		_createDate = remoteIssue.created;
		_dueDate = remoteIssue.duedate;
		_updateDate = remoteIssue.updated;
		_resolutionDate = remoteIssue.resolutionDateReadOnly;
		_securityLevel = remoteIssue.securityLevelReadOnly;

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
			_parentIssueKey = remoteIssue.parentKey;
		}

		// named entities
		_status = remoteIssue.status == null ? null : new IssueStatus(remoteIssue.status);
		Priority = remoteIssue.priority == null ? null : new IssuePriority(remoteIssue.priority);
		Resolution = remoteIssue.resolution == null ? null : new IssueResolution(remoteIssue.resolution);
		Type = remoteIssue.type == null ? null : new IssueType(remoteIssue.type);

		// collections
		_customFields = _originalIssue.customFieldValues == null ? new CustomFieldValueCollection(this)
			: new CustomFieldValueCollection(this, [.. _originalIssue.customFieldValues.Select(f => new CustomFieldValue(f.customfieldId, this) { Values = f.values, RawValue = f.rawValue })]);

		var affectsVersions = _originalIssue.affectsVersions ?? Enumerable.Empty<RemoteVersion>();
		_affectsVersions = new ProjectVersionCollection("versions", _jira, Project, [.. affectsVersions.Select(v =>
		{
			v.ProjectKey = _originalIssue.project;
			return new ProjectVersion(_jira, v);
		})]);

		var fixVersions = _originalIssue.fixVersions ?? Enumerable.Empty<RemoteVersion>();
		_fixVersions = new ProjectVersionCollection("fixVersions", _jira, Project, [.. fixVersions.Select(v =>
		{
			v.ProjectKey = _originalIssue.project;
			return new ProjectVersion(_jira, v);
		})]);

		var labels = _originalIssue.labels ?? [];
		_labels = new IssueLabelCollection([.. labels]);

		var components = _originalIssue.components ?? Enumerable.Empty<RemoteComponent>();
		_components = new ProjectComponentCollection("components", _jira, Project, [.. components.Select(c =>
		{
			c.ProjectKey = _originalIssue.project;
			return new ProjectComponent(c);
		})]);

		// additional fields
		AdditionalFields = new IssueFields(_originalIssue, Jira);
	}

	internal RemoteIssue OriginalRemoteIssue
	{
		get
		{
			return _originalIssue;
		}
	}

	/// <summary>
	/// Fields not represented as properties that were retrieved from the server.
	/// </summary>
	public IssueFields AdditionalFields { get; private set; }

	/// <summary>
	/// The parent key if this issue is a subtask.
	/// </summary>
	/// <remarks>
	/// Only available if issue was retrieved using REST API.
	/// </remarks>
	public string ParentIssueKey
	{
		get { return _parentIssueKey; }
	}

	/// <summary>
	/// The JIRA server that created this issue
	/// </summary>
	public JiraClient Jira
	{
		get
		{
			return _jira;
		}
	}

	/// <summary>
	/// Gets the security level set on the issue.
	/// </summary>
	public IssueSecurityLevel SecurityLevel
	{
		get
		{
			return _securityLevel;
		}
	}

	/// <summary>
	/// Brief one-line summary of the issue
	/// </summary>
	[JqlContainsEquality]
	public string Summary { get; set; }

	/// <summary>
	/// Detailed description of the issue
	/// </summary>
	[JqlContainsEquality]
	public string Description { get; set; }

	/// <summary>
	/// Hardware or software environment to which the issue relates.
	/// </summary>
	[JqlContainsEquality]
	public string Environment { get; set; }

	/// <summary>
	/// Username or account id of user to whom the issue is currently assigned.
	/// </summary>
	public string Assignee { get; set; }

	/// <summary>
	/// User object of user to whom the issue is currently assigned.
	/// </summary>
	public JiraUser AssigneeUser { get; private set; }

	/// <summary>
	/// Time tracking data for this issue.
	/// </summary>
	public IssueTimeTrackingData TimeTrackingData { get; private set; }

	/// <summary>
	/// Gets the internal identifier assigned by JIRA.
	/// </summary>
	public string JiraIdentifier
	{
		get
		{
			if (string.IsNullOrEmpty(_originalIssue.key))
			{
				throw new InvalidOperationException("Unable to retrieve JIRA id, issue has not been created.");
			}

			return _originalIssue.id;
		}
	}

	/// <summary>
	/// Unique identifier for this issue
	/// </summary>
	public ComparableString Key
	{
		get
		{
			return _key;
		}
	}

	/// <summary>
	/// Importance of the issue in relation to other issues
	/// </summary>
	public IssuePriority Priority { get; set; }

	/// <summary>
	/// Parent project to which the issue belongs
	/// </summary>
	public string Project
	{
		get
		{
			return _project;
		}
	}

	/// <summary>
	/// Username or account id of user who created the issue in Jira.
	/// </summary>
	public string Reporter { get; set; }

	/// <summary>
	/// User object of user who created the issue in Jira.
	/// </summary>
	public JiraUser ReporterUser { get; private set; }

	/// <summary>
	/// Record of the issue's resolution, if the issue has been resolved or closed
	/// </summary>
	public IssueResolution Resolution { get; set; }

	/// <summary>
	/// The stage the issue is currently at in its lifecycle.
	/// </summary>
	public IssueStatus Status
	{
		get
		{
			return _status;
		}
	}

	/// <summary>
	/// The type of the issue
	/// </summary>
	[RemoteFieldName("issuetype")]
	public IssueType Type
	{
		get
		{
			return _issueType;
		}
		set
		{
			_issueType = value;

			if (_issueType != null)
			{
				_issueType.ProjectKey = _project;
			}
		}
	}

	/// <summary>
	/// Number of votes the issue has
	/// </summary>
	public long? Votes { get; private set; }

	/// <summary>
	/// Whether the user that retrieved this issue has voted on it.
	/// </summary>
	public bool HasUserVoted { get; private set; }

	/// <summary>
	/// Time and date on which this issue was entered into JIRA
	/// </summary>
	public DateTime? Created
	{
		get
		{
			return _createDate;
		}
	}

	/// <summary>
	/// Date by which this issue is scheduled to be completed
	/// </summary>
	public DateTime? DueDate
	{
		get
		{
			return _dueDate;
		}
		set
		{
			_dueDate = value;
		}
	}

	/// <summary>
	/// Time and date on which this issue was last edited
	/// </summary>
	public DateTime? Updated
	{
		get
		{
			return _updateDate;
		}
	}

	/// <summary>
	/// Time and date on which this issue was resolved.
	/// </summary>
	/// <remarks>
	/// Only available if issue was retrieved using REST API, use GetResolutionDate
	/// method for SOAP clients.
	/// </remarks>
	public DateTime? ResolutionDate
	{
		get
		{
			return _resolutionDate;
		}
	}

	/// <summary>
	/// The components associated with this issue
	/// </summary>
	[JqlFieldName("component")]
	public ProjectComponentCollection Components
	{
		get
		{
			return _components;
		}
	}

	/// <summary>
	/// The versions that are affected by this issue
	/// </summary>
	[JqlFieldName("AffectedVersion")]
	public ProjectVersionCollection AffectsVersions
	{
		get
		{
			return _affectsVersions;
		}
	}

	/// <summary>
	/// The versions in which this issue is fixed
	/// </summary>
	[JqlFieldName("FixVersion")]
	public ProjectVersionCollection FixVersions
	{
		get
		{
			return _fixVersions;
		}
	}

	/// <summary>
	/// The labels assigned to this issue.
	/// </summary>
	public IssueLabelCollection Labels
	{
		get
		{
			return _labels;
		}
	}

	/// <summary>
	/// The custom fields associated with this issue
	/// </summary>
	public CustomFieldValueCollection CustomFields
	{
		get
		{
			return _customFields;
		}
	}

	/// <summary>
	/// Gets or sets the value of a custom field
	/// </summary>
	/// <param name="customFieldName">Custom field name</param>
	/// <returns>Value of the custom field</returns>
	public ComparableString? this[string customFieldName]
	{
		get
		{
			var customField = _customFields[customFieldName];

			if (customField != null && customField.Values != null && customField.Values.Length > 0)
			{
				return customField.Values[0];
			}

			return null;
		}
		set
		{
			var customField = _customFields[customFieldName];
			string[] customFieldValue = value == null ? null : [value.Value];

			if (customField != null)
			{
				customField.Values = customFieldValue;
			}
			else
			{
				_customFields.AddArrayAsync(customFieldName, customFieldValue, default).GetAwaiter().GetResult();
			}
		}
	}

	/// <summary>
	/// Saves field changes to server.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public async Task<Issue> SaveChangesAsync(CancellationToken cancellationToken)
	{
		Issue serverIssue;
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			var newKey = await _jira.Issues.CreateIssueAsync(this, cancellationToken).ConfigureAwait(false);
			serverIssue = await _jira.Issues.GetIssueAsync(newKey, cancellationToken).ConfigureAwait(false);
		}
		else
		{
			await _jira.Issues.UpdateIssueAsync(this, cancellationToken).ConfigureAwait(false);
			serverIssue = await _jira.Issues.GetIssueAsync(_originalIssue.key, cancellationToken).ConfigureAwait(false);
		}

		Initialize(serverIssue.OriginalRemoteIssue);
		return serverIssue;
	}

	/// <summary>
	/// Creates a link between this issue and the issue specified.
	/// </summary>
	/// <param name="inwardIssueKey">Key of the issue to link.</param>
	/// <param name="linkName">Name of the issue link type.</param>
	/// <param name="comment">Comment to add to this issue.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task LinkToIssueAsync(string inwardIssueKey, string linkName, string? comment, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to link issue, issue has not been created.");
		}

		return Jira.Links.CreateLinkAsync(Key.Value, inwardIssueKey, linkName, comment, cancellationToken);
	}

	/// <summary>
	/// Gets the issue links associated with this issue.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<IEnumerable<IssueLink>> GetIssueLinksAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to get issue links issues, issue has not been created.");
		}

		return Jira.Links.GetLinksForIssueAsync(this, null, cancellationToken);
	}

	/// <summary>
	/// Gets the issue links associated with this issue.
	/// </summary>
	/// <param name="linkTypeNames">Optional subset of link types to retrieve.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<IEnumerable<IssueLink>> GetIssueLinksAsync(IEnumerable<string> linkTypeNames, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to get issue links issues, issue has not been created.");
		}

		return Jira.Links.GetLinksForIssueAsync(this, linkTypeNames, cancellationToken);
	}

	/// <summary>
	/// Creates an remote link for an issue.
	/// </summary>
	/// <param name="remoteUrl">Remote url to link to.</param>
	/// <param name="title">Title of the remote link.</param>
	/// <param name="summary">Summary of the remote link.</param>
	/// <param name="cancellationToken"></param>
	public Task AddRemoteLinkAsync(string remoteUrl, string title, string? summary, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to add remote link, issue has not been created.");
		}

		return Jira.RemoteLinks.CreateRemoteLinkAsync(Key.Value, remoteUrl, title, summary, cancellationToken);
	}

	/// <summary>
	/// Gets the remote links associated with this issue.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<IEnumerable<IssueRemoteLink>> GetRemoteLinksAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to get remote links, issue has not been created.");
		}

		return Jira.RemoteLinks.GetRemoteLinksForIssueAsync(_originalIssue.key, cancellationToken);
	}

	/// <summary>
	/// Transition an issue through a workflow action.
	/// </summary>
	/// <param name="actionNameOrId">The workflow action name or id to transition to.</param>
	/// <param name="additionalUpdates">Additional updates to perform when transitioning the issue.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public async Task WorkflowTransitionAsync(string actionNameOrId, WorkflowTransitionUpdates? additionalUpdates, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to execute workflow transition, issue has not been created.");
		}

		await _jira.Issues.ExecuteWorkflowActionAsync(this, actionNameOrId, additionalUpdates, cancellationToken).ConfigureAwait(false);
		var issue = await _jira.Issues.GetIssueAsync(_originalIssue.key, cancellationToken).ConfigureAwait(false);
		Initialize(issue.OriginalRemoteIssue);
	}

	/// <summary>
	/// Returns the issues that are marked as sub tasks of this issue.
	/// </summary>
	/// <param name="skip">Index of the first issue to return (0-based).</param>
	/// <param name="take">Maximum number of issues to retrieve.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<IPagedQueryResult<Issue>> GetSubTasksAsync(int skip, int? take, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to retrieve subtasks from server, issue has not been created.");
		}

		return _jira.Issues.GetSubTasksAsync(_originalIssue.key, skip, take, cancellationToken);
	}

	/// <summary>
	/// Retrieve attachment metadata from server for this issue
	/// </summary>
	public Task<IEnumerable<Attachment>> GetAttachmentsAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to retrieve attachments from server, issue has not been created.");
		}

		return Jira.Issues.GetAttachmentsAsync(Key.Value, cancellationToken);
	}

	/// <summary>
	/// Add one or more attachments to this issue
	/// </summary>
	/// <param name="fileInfos">FileInfos of files to upload</param>
	/// <param name="cancellationToken"></param>
	public async Task AddAttachmentAsync(FileInfo[] fileInfos, CancellationToken cancellationToken)
	{
		var attachments = fileInfos.Select(f => new UploadAttachmentInfo(f.Name, _jira.FileSystem.FileReadAllBytes(f.FullName))).ToArray();

		await AddAttachmentAsync(attachments, cancellationToken);
	}

	/// <summary>
	/// Add an attachment to this issue
	/// </summary>
	/// <param name="fileInfo">FileInfo of file to upload</param>
	/// <param name="cancellationToken"></param>
	public Task AddAttachmentAsync(FileInfo fileInfo, CancellationToken cancellationToken)
		=> AddAttachmentAsync([fileInfo], cancellationToken);

	/// <summary>
	/// Add an attachment to this issue
	/// </summary>
	/// <param name="cancellationToken"></param>
	public Task AddAttachmentAsync(string name, byte[] data, CancellationToken cancellationToken)
		=> AddAttachmentAsync([new UploadAttachmentInfo(name, data)], cancellationToken);

	/// <summary>
	/// Add one or more attachments to this issue.
	/// </summary>
	/// <param name="attachments">Attachment objects that describe the files to upload.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task AddAttachmentAsync(UploadAttachmentInfo[] attachments, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to upload attachments to server, issue has not been created.");
		}

		return _jira.Issues.AddAttachmentsAsync(_originalIssue.key, attachments, cancellationToken);
	}

	/// <summary>
	/// Removes an attachment from this issue.
	/// </summary>
	/// <param name="attachment">Attachment to remove.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task DeleteAttachmentAsync(Attachment attachment, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to delete attachment from server, issue has not been created.");
		}

		return _jira.Issues.DeleteAttachmentAsync(_originalIssue.key, attachment.Id, cancellationToken);
	}

	/// <summary>
	/// Gets a dictionary with issue field names as keys and their metadata as values.
	/// </summary>
	public Task<IDictionary<string, IssueFieldEditMetadata>> GetIssueFieldsEditMetadataAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to retrieve issue fields from server, make sure the issue has been created.");
		}

		return _jira.Issues.GetFieldsEditMetadataAsync(_originalIssue.key, cancellationToken);
	}

	/// <summary>
	/// Retrieve change logs from server for this issue.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<IEnumerable<IssueChangeLog>> GetChangeLogsAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to retrieve change logs from server, issue has not been created.");
		}

		return _jira.Issues.GetChangeLogsAsync(_originalIssue.key, cancellationToken);
	}

	/// <summary>
	/// Get the comments for this issue.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<IEnumerable<Comment>> GetCommentsAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to retrieve comments from server, issue has not been created.");
		}

		return _jira.Issues.GetCommentsAsync(_originalIssue.key, cancellationToken);
	}

	/// <summary>
	/// Get the comments for this issue.
	/// </summary>
	/// <param name="options">Options to use when querying the comments.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<IEnumerable<Comment>> GetCommentsAsync(CommentQueryOptions options, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to retrieve comments from server, issue has not been created.");
		}

		return _jira.Issues.GetCommentsAsync(_originalIssue.key, options, cancellationToken);
	}

	/// <summary>
	/// Get the comments for this issue.
	/// </summary>
	/// <param name="skip">Index of the first comment to return (0-based).</param>
	/// <param name="take">Maximum number of comments to retrieve.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<IPagedQueryResult<Comment>> GetPagedCommentsAsync(
		int skip,
		int? take,
		CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to retrieve comments from server, issue has not been created.");
		}

		return Jira.Issues.GetPagedCommentsAsync(Key.Value, skip, take, cancellationToken);
	}

	/// <summary>
	/// Add a comment to this issue.
	/// </summary>
	/// <param name="comment">Comment text to add.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public async Task<Comment> AddCommentAsync(string comment, CancellationToken cancellationToken)
	{
		var jiraUser = await Jira.Users.GetMyselfAsync(cancellationToken);

		var author = Jira.RestClient.Settings.EnableUserPrivacyMode ? jiraUser.AccountId : jiraUser.Username;

		return await AddCommentAsync(new Comment() { Author = author, Body = comment }, cancellationToken);
	}

	/// <summary>
	/// Removes a comment from this issue.
	/// </summary>
	/// <param name="comment">Comment to remove.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task DeleteCommentAsync(Comment comment, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to delete comment from server, issue has not been created.");
		}

		return _jira.Issues.DeleteCommentAsync(_originalIssue.key, comment.Id, cancellationToken);
	}

	/// <summary>
	/// Add a comment to this issue.
	/// </summary>
	/// <param name="comment">Comment object to add.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<Comment> AddCommentAsync(Comment comment, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to add comment to issue, issue has not been created.");
		}

		return Jira.Issues.AddCommentAsync(Key.Value, comment, cancellationToken);
	}

	/// <summary>
	/// Update a comment in this issue.
	/// </summary>
	/// <param name="comment">Comment object to update.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<Comment> UpdateCommentAsync(Comment comment, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to update comment to issue, issue has not been created.");
		}

		return Jira.Issues.UpdateCommentAsync(Key.Value, comment, cancellationToken);
	}

	/// <summary>
	/// Retrieve the labels from server for this issue.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	[Obsolete("Use Issue.Labels instead.")]
	public Task<string[]> GetLabelsAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to get labels from issue, issue has not been created.");
		}

		return Jira.Issues.GetLabelsAsync(_originalIssue.key, cancellationToken);
	}

	/// <summary>
	///  Adds a worklog to this issue.
	/// </summary>
	/// <param name="timeSpent">Specifies a time duration in JIRA duration format, representing the time spent working on the worklog</param>
	/// <param name="worklogStrategy">How to handle the remaining estimate, recommend using AutoAdjustRemainingEstimate</param>
	/// <param name="newEstimate">New estimate (only used if worklogStrategy set to NewRemainingEstimate)</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Worklog as constructed by server</returns>
	public Task<Worklog> AddWorklogAsync(
		string timeSpent,
		WorklogStrategy worklogStrategy,
		string? newEstimate,
		CancellationToken cancellationToken)
	{
		return AddWorklogAsync(new Worklog(timeSpent, DateTime.Now), worklogStrategy, newEstimate, cancellationToken);
	}

	/// <summary>
	///  Adds a worklog to this issue.
	/// </summary>
	/// <param name="worklog">The worklog instance to add</param>
	/// <param name="worklogStrategy">How to handle the remaining estimate, recommend using AutoAdjustRemainingEstimate</param>
	/// <param name="newEstimate">New estimate (only used if worklogStrategy set to NewRemainingEstimate)</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	/// <returns>Worklog as constructed by server</returns>
	public Task<Worklog> AddWorklogAsync(
		Worklog worklog,
		WorklogStrategy worklogStrategy,
		string? newEstimate,
		CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to add worklog to issue, issue has not been saved to server.");
		}

		return _jira.Issues.AddWorklogAsync(_originalIssue.key, worklog, worklogStrategy, newEstimate, cancellationToken);
	}

	/// <summary>
	/// Deletes the given worklog from the issue and updates the remaining estimate field.
	/// </summary>
	/// <param name="worklog">The worklog to remove.</param>
	/// <param name="worklogStrategy">How to handle the remaining estimate, suggest AutoAdjustRemainingEstimate.</param>
	/// <param name="newEstimate">New estimate (only used if worklogStrategy set to NewRemainingEstimate)</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task DeleteWorklogAsync(
		Worklog worklog,
		WorklogStrategy worklogStrategy,
		string? newEstimate,
		CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to delete worklog from issue, issue has not been saved to server.");
		}

		return _jira.Issues.DeleteWorklogAsync(_originalIssue.key, worklog.Id, worklogStrategy, newEstimate, cancellationToken);
	}

	/// <summary>
	/// Retrieve worklogs for this issue.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<IEnumerable<Worklog>> GetWorklogsAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to retrieve worklogs, issue has not been saved to server.");
		}

		return _jira.Issues.GetWorklogsAsync(_originalIssue.key, cancellationToken);
	}

	/// <summary>
	/// Updates all fields from server.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public async Task RefreshAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to refresh, issue has not been saved to server.");
		}

		var serverIssue = await _jira.Issues.GetIssueAsync(_originalIssue.key, cancellationToken).ConfigureAwait(false);
		Initialize(serverIssue.OriginalRemoteIssue);
	}

	/// <summary>
	/// Gets the workflow actions that the issue can be transitioned to.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<IEnumerable<IssueTransition>> GetAvailableActionsAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to retrieve actions, issue has not been saved to server.");
		}

		return _jira.Issues.GetActionsAsync(_originalIssue.key, cancellationToken);
	}

	/// <summary>
	/// Gets the workflow actions that the issue can be transitioned to including the fields that are required per action.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<IEnumerable<IssueTransition>> GetAvailableActionsAsync(bool expandTransitionFields, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to retrieve actions, issue has not been saved to server.");
		}

		return _jira.Issues.GetActionsAsync(_originalIssue.key, expandTransitionFields, cancellationToken);
	}

	/// <summary>
	/// Gets time tracking information for this issue.
	/// </summary>
	/// <remarks>
	/// - Returns information as it was at the moment the issue was read from server, to get latest data use the GetTimeTrackingData method.
	/// - Use the AddWorklog methods to edit the time tracking information.
	/// </remarks>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<IssueTimeTrackingData> GetTimeTrackingDataAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to retrieve time tracking data, issue has not been saved to server.");
		}

		return _jira.Issues.GetTimeTrackingDataAsync(_originalIssue.key, cancellationToken);
	}

	/// <summary>
	/// Adds a user to the watchers of the issue.
	/// </summary>
	/// <param name="usernameOrAccountId">Username or account id of the user to add as a watcher.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task AddWatcherAsync(string usernameOrAccountId, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to add watcher, issue has not been saved to server.");
		}

		return _jira.Issues.AddWatcherAsync(_originalIssue.key, usernameOrAccountId, cancellationToken);
	}

	/// <summary>
	/// Gets the users that are watching the issue.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task<IEnumerable<JiraUser>> GetWatchersAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to get watchers, issue has not been saved to server.");
		}

		return _jira.Issues.GetWatchersAsync(_originalIssue.key, cancellationToken);
	}

	/// <summary>
	/// Removes a user from the watchers of the issue.
	/// </summary>
	/// <param name="usernameOrAccountId">Username or account id of the user to remove as a watcher.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task DeleteWatcherAsync(string usernameOrAccountId, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to remove watcher, issue has not been saved to server.");
		}

		return _jira.Issues.DeleteWatcherAsync(_originalIssue.key, usernameOrAccountId, cancellationToken);
	}

	/// <summary>
	/// Assigns this issue to the specified user.
	/// </summary>
	/// <param name="assignee">The username or account id of the user to assign this issue to.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public async Task AssignAsync(string assignee, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to assign issue, issue has not been saved to server.");
		}

		await _jira.Issues.AssignIssueAsync(Key.Value, assignee, cancellationToken);
		var issue = await _jira.Issues.GetIssueAsync(_originalIssue.key, cancellationToken).ConfigureAwait(false);
		Initialize(issue.OriginalRemoteIssue);
	}

	/// <summary>
	/// Fetches the requested properties and their mapping.
	/// </summary>
	/// <remarks>
	/// Property keys yielded by <paramref name="propertyKeys"/> must have a length between 0 and 256 (both exclusive).
	/// </remarks>
	/// <param name="propertyKeys">Enumeration of requested property keys.</param>
	/// <param name="cancellationToken">Asynchronous operation control token.</param>
	/// <returns>A dictionary of property values mapped to their keys.</returns>
	public Task<ReadOnlyDictionary<string, JToken>> GetPropertiesAsync(IEnumerable<string> propertyKeys, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to fetch issue properties, issue has not been saved to server.");
		}

		return _jira.Issues.GetPropertiesAsync(_originalIssue.key, propertyKeys, cancellationToken);
	}

	/// <summary>
	/// Sets the value of the specified property key. The key is created if it didn't already exist.
	/// </summary>
	/// <remarks>
	/// The property key (<paramref name="propertyKey"/>) must have a length between 0 and 256 (both exclusive).
	/// </remarks>
	/// <param name="propertyKey">The property key.</param>
	/// <param name="obj">The value to store.</param>
	/// <param name="cancellationToken">Asynchronous operation control token.</param>
	public Task SetPropertyAsync(string propertyKey, JToken obj, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to add issue properties, issue has not been saved to server.");
		}

		return _jira.Issues.SetPropertyAsync(_originalIssue.key, propertyKey, obj, cancellationToken);
	}

	/// <summary>
	/// Remove the specified property key and its stored value.
	/// </summary>
	/// <remarks>
	/// The property key (<paramref name="propertyKey"/>) must have a length between 0 and 256 (both exclusive).
	/// </remarks>
	/// <param name="propertyKey">The property key.</param>
	/// <param name="cancellationToken">Asynchronous operation control token.</param>
	public Task DeletePropertyAsync(string propertyKey, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(_originalIssue.key))
		{
			throw new InvalidOperationException("Unable to remove issue properties, issue has not been saved to server.");
		}

		return _jira.Issues.DeletePropertyAsync(_originalIssue.key, propertyKey, cancellationToken);
	}

	/// <summary>
	/// Gets the RemoteFields representing the fields that were updated
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	async Task<RemoteFieldValue[]> IRemoteIssueFieldProvider.GetRemoteFieldValuesAsync(CancellationToken cancellationToken)
	{
		var fields = new List<RemoteFieldValue>();

		var remoteFields = typeof(RemoteIssue).GetProperties();
		foreach (var localProperty in typeof(Issue).GetProperties())
		{
			if (typeof(IRemoteIssueFieldProvider).IsAssignableFrom(localProperty.PropertyType))
			{
				if (localProperty.GetValue(this, null) is IRemoteIssueFieldProvider fieldsProvider)
				{
					var remoteFieldValues = await fieldsProvider.GetRemoteFieldValuesAsync(cancellationToken).ConfigureAwait(false);
					fields.AddRange(remoteFieldValues);
				}
			}
			else
			{
				var remoteProperty = remoteFields.FirstOrDefault(i => i.Name.Equals(localProperty.Name, StringComparison.OrdinalIgnoreCase));
				if (remoteProperty == null)
				{
					continue;
				}

				var localStringValue = await GetStringValueForPropertyAsync(this, localProperty, cancellationToken).ConfigureAwait(false);
				var remoteStringValue = await GetStringValueForPropertyAsync(_originalIssue, remoteProperty, cancellationToken).ConfigureAwait(false);

				if (remoteStringValue != localStringValue)
				{
					var remoteFieldName = remoteProperty.Name;

					var remoteFieldNameAttr = localProperty.GetCustomAttributes(typeof(RemoteFieldNameAttribute), true).OfType<RemoteFieldNameAttribute>().FirstOrDefault();
					if (remoteFieldNameAttr != null)
					{
						remoteFieldName = remoteFieldNameAttr.Name;
					}

					fields.Add(new RemoteFieldValue()
					{
						id = remoteFieldName,
						values = [localStringValue]
					});
				}
			}
		}

		return [.. fields];
	}

	internal async Task<RemoteIssue> ToRemoteAsync(CancellationToken cancellationToken)
	{
		var remote = new RemoteIssue
		{
			assignee = Assignee,
			description = Description,
			environment = Environment,
			project = Project,
			reporter = Reporter,
			summary = Summary,
			votesData = Votes != null ? new RemoteVotes() { hasVoted = HasUserVoted == true, votes = Votes.Value } : null,
			duedate = DueDate,
			timeTracking = TimeTrackingData,
			key = Key?.Value
		};

		if (Status != null)
		{
			await Status.LoadIdAndNameAsync(_jira, cancellationToken).ConfigureAwait(false);
			remote.status = new RemoteStatus() { id = Status.Id, name = Status.Name };
		}

		if (Resolution != null)
		{
			await Resolution.LoadIdAndNameAsync(_jira, cancellationToken).ConfigureAwait(false);
			remote.resolution = new RemoteResolution() { id = Resolution.Id, name = Resolution.Name };
		}

		if (Priority != null)
		{
			await Priority.LoadIdAndNameAsync(_jira, cancellationToken).ConfigureAwait(false);
			remote.priority = new RemotePriority() { id = Priority.Id, name = Priority.Name };
		}

		if (Type != null)
		{
			await Type.LoadIdAndNameAsync(_jira, cancellationToken).ConfigureAwait(false);
			remote.type = new RemoteIssueType() { id = Type.Id, name = Type.Name };
		}

		if (AffectsVersions.Count > 0)
		{
			remote.affectsVersions = [.. AffectsVersions.Select(v => v.RemoteVersion)];
		}

		if (FixVersions.Count > 0)
		{
			remote.fixVersions = [.. FixVersions.Select(v => v.RemoteVersion)];
		}

		if (Components.Count > 0)
		{
			remote.components = [.. Components.Select(c => c.RemoteComponent)];
		}

		if (CustomFields.Count > 0)
		{
			remote.customFieldValues = [.. CustomFields.Select(f => new RemoteCustomFieldValue()
			{
				customfieldId = f.Id,
				values = f.Values,
				serializer = f.Serializer
			})];
		}

		if (Labels.Count > 0)
		{
			remote.labels = [.. Labels];
		}

		return remote;
	}

	private async Task<string> GetStringValueForPropertyAsync(object container, PropertyInfo property, CancellationToken cancellationToken)
	{
		var value = property.GetValue(container, null);

		if (property.PropertyType == typeof(DateTime?))
		{
			var dateValue = (DateTime?)value;
			return dateValue.HasValue ? dateValue.Value.ToString("d/MMM/yy") : null;
		}
		else if (typeof(JiraNamedEntity).IsAssignableFrom(property.PropertyType))
		{
			if (property.GetValue(container, null) is JiraNamedEntity jiraNamedEntity)
			{
				await jiraNamedEntity.LoadIdAndNameAsync(_jira, cancellationToken).ConfigureAwait(false);
				return jiraNamedEntity.Id;
			}

			return null;
		}
		else if (typeof(AbstractNamedRemoteEntity).IsAssignableFrom(property.PropertyType))
		{
			if (property.GetValue(container, null) is AbstractNamedRemoteEntity remoteEntity)
			{
				return remoteEntity.id;
			}

			return null;
		}
		else
		{
			return value?.ToString();
		}
	}
}
