using Jira.Api.Remote;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api;

/// <summary>
/// An attachment associated with an issue
/// </summary>
/// <remarks>
/// Creates a new instance of an Attachment from a remote entity.
/// </remarks>
/// <param name="jira">Object used to interact with JIRA.</param>
/// <param name="remoteAttachment">Remote attachment entity.</param>
public class Attachment(JiraClient jira, RemoteAttachment remoteAttachment)
{
	private readonly JiraClient _jira = jira;

	/// <summary>
	/// Id of attachment
	/// </summary>
	public string Id { get; private set; } = remoteAttachment.id;

	/// <summary>
	/// Author of attachment (user that uploaded the file)
	/// </summary>
	public string Author
	{
		get
		{
			return AuthorUser?.InternalIdentifier;
		}
	}

	/// <summary>
	/// User object of the author of attachment.
	/// </summary>
	public JiraUser AuthorUser { get; private set; } = remoteAttachment.authorUser;

	/// <summary>
	/// Date of creation
	/// </summary>
	public DateTime? CreatedDate { get; private set; } = remoteAttachment.created;

	/// <summary>
	/// File name of the attachment
	/// </summary>
	public string FileName { get; private set; } = remoteAttachment.filename;

	/// <summary>
	/// Mime type
	/// </summary>
	public string MimeType { get; private set; } = remoteAttachment.mimetype;

	/// <summary>
	/// File size
	/// </summary>
	public long? FileSize { get; private set; } = remoteAttachment.filesize;

	/// <summary>
	/// Downloads attachment as a byte array.
	/// </summary>
	public async Task<byte[]?> DownloadDataAsync(CancellationToken cancellationToken)
	{
		var url = GetRequestUrl();

		return await _jira.RestClient.DownloadDataAsync(url, cancellationToken);
	}

	/// <summary>
	/// Downloads attachment to specified file
	/// </summary>
	/// <param name="fullFileName">Full file name where attachment will be downloaded</param>
	public async Task DownloadAsync(string fullFileName, CancellationToken cancellationToken)
	{
		var url = GetRequestUrl();

		await _jira.RestClient.DownloadAsync(url, fullFileName, cancellationToken);
	}

	private string GetRequestUrl()
	{
		if (string.IsNullOrEmpty(_jira.Url))
		{
			throw new InvalidOperationException("Unable to download attachment, JIRA url has not been set.");
		}

		return $"{(_jira.Url.EndsWith('/') ? _jira.Url : _jira.Url + "/")}secure/attachment/{Id}/{FileName}";
	}
}
