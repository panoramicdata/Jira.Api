using System;
using Jira.Api.Remote;

namespace Jira.Api;

/// <summary>
/// An attachment associated with an issue
/// </summary>
/// <remarks>
/// Creates a new instance of an Attachment from a remote entity.
/// </remarks>
/// <param name="jira">Object used to interact with JIRA.</param>
/// <param name="remoteAttachment">Remote attachment entity.</param>
public class Attachment(Jira jira, RemoteAttachment remoteAttachment)
{
	private readonly Jira _jira = jira;

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
	public byte[] DownloadData()
	{
		var url = GetRequestUrl();

		return _jira.RestClient.DownloadData(url);
	}

	/// <summary>
	/// Downloads attachment to specified file
	/// </summary>
	/// <param name="fullFileName">Full file name where attachment will be downloaded</param>
	public void Download(string fullFileName)
	{
		var url = GetRequestUrl();

		_jira.RestClient.Download(url, fullFileName);
	}

	private string GetRequestUrl()
	{
		if (string.IsNullOrEmpty(_jira.Url))
		{
			throw new InvalidOperationException("Unable to download attachment, JIRA url has not been set.");
		}

		return string.Format("{0}secure/attachment/{1}/{2}",
			_jira.Url.EndsWith("/") ? _jira.Url : _jira.Url + "/",
			this.Id,
			this.FileName);
	}
}
