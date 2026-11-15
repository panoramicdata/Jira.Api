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
	/// Saves field changes to server.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	public async Task<Issue> SaveChangesAsync(CancellationToken cancellationToken)
	{
		Issue serverIssue;
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
		{
			var newKey = await Jira.Issues.CreateIssueAsync(this, cancellationToken).ConfigureAwait(false);
			serverIssue = await Jira.Issues.GetIssueAsync(newKey, cancellationToken).ConfigureAwait(false);
		}
		else
		{
			await Jira.Issues.UpdateIssueAsync(this, cancellationToken).ConfigureAwait(false);
			serverIssue = await Jira.Issues.GetIssueAsync(OriginalRemoteIssue.key, cancellationToken).ConfigureAwait(false);
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
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
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
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
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
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
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
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
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
		if (string.IsNullOrEmpty(OriginalRemoteIssue.key))
		{
			throw new InvalidOperationException("Unable to get remote links, issue has not been created.");
		}

		return Jira.RemoteLinks.GetRemoteLinksForIssueAsync(OriginalRemoteIssue.key, cancellationToken);
	}
}
