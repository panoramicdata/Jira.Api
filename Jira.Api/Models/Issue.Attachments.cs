using Jira.Api.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;

namespace Jira.Api.Models;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2360:Optional parameters should not be used", Justification = "Published public API: replacing optional parameters with overloads is a breaking change for existing consumers.")]
public partial class Issue
{
	/// <summary>
	/// Transition an issue through a workflow action.
	/// </summary>
	/// <param name="actionNameOrId">The workflow action name or id to transition to.</param>
	/// <param name="additionalUpdates">Additional updates to perform when transitioning the issue.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public async Task WorkflowTransitionAsync(string actionNameOrId, WorkflowTransitionUpdates? additionalUpdates, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
		{
			throw new InvalidOperationException("Unable to execute workflow transition, issue has not been created.");
		}

		await Jira.Issues.ExecuteWorkflowActionAsync(this, actionNameOrId, additionalUpdates, cancellationToken).ConfigureAwait(false);
		var issue = await Jira.Issues.GetIssueAsync(OriginalRemoteIssue.key, cancellationToken).ConfigureAwait(false);
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
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
		{
			throw new InvalidOperationException("Unable to retrieve subtasks from server, issue has not been created.");
		}

		return Jira.Issues.GetSubTasksAsync(OriginalRemoteIssue.key, skip, take, cancellationToken);
	}

	/// <summary>
	/// Retrieve attachment metadata from server for this issue
	/// </summary>
	public Task<IEnumerable<Attachment>> GetAttachmentsAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
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
		var attachments = fileInfos.Select(f => new UploadAttachmentInfo(f.Name, Jira.FileSystem.FileReadAllBytes(f.FullName))).ToArray();

		await AddAttachmentAsync(attachments, cancellationToken);
	}

	/// <summary>
	/// Add an attachment to this issue
	/// </summary>
	/// <param name="fileInfo">FileInfo of file to upload</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task AddAttachmentAsync(FileInfo fileInfo, CancellationToken cancellationToken)
		=> AddAttachmentAsync([fileInfo], cancellationToken);

	/// <summary>
	/// Add an attachment to this issue
	/// </summary>
	/// <param name="name">The name of the attachment file.</param>
	/// <param name="data">The byte array containing the attachment data.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task AddAttachmentAsync(string name, byte[] data, CancellationToken cancellationToken)
		=> AddAttachmentAsync([new UploadAttachmentInfo(name, data)], cancellationToken);

	/// <summary>
	/// Add one or more attachments to this issue.
	/// </summary>
	/// <param name="attachments">Attachment objects that describe the files to upload.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task AddAttachmentAsync(UploadAttachmentInfo[] attachments, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
		{
			throw new InvalidOperationException("Unable to upload attachments to server, issue has not been created.");
		}

		return Jira.Issues.AddAttachmentsAsync(OriginalRemoteIssue.key, attachments, cancellationToken);
	}

	/// <summary>
	/// Removes an attachment from this issue.
	/// </summary>
	/// <param name="attachment">Attachment to remove.</param>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public Task DeleteAttachmentAsync(Attachment attachment, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
		{
			throw new InvalidOperationException("Unable to delete attachment from server, issue has not been created.");
		}

		return Jira.Issues.DeleteAttachmentAsync(OriginalRemoteIssue.key, attachment.Id, cancellationToken);
	}

	/// <summary>
	/// Gets a dictionary with issue field names as keys and their metadata as values.
	/// </summary>
	public Task<IDictionary<string, IssueFieldEditMetadata>> GetIssueFieldsEditMetadataAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
		{
			throw new InvalidOperationException("Unable to retrieve issue fields from server, make sure the issue has been created.");
		}

		return Jira.Issues.GetFieldsEditMetadataAsync(OriginalRemoteIssue.key, cancellationToken);
	}
}
