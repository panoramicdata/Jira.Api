namespace Jira.Api;

/// <summary>
/// Information about an attachment to be uploaded
/// </summary>
public class UploadAttachmentInfo(string name, byte[] data)
{
	/// <summary>
	/// The name of the attachment
	/// </summary>
	public string Name { get; set; } = name;

	/// <summary>
	/// The binary data of the attachment
	/// </summary>
	public byte[] Data { get; set; } = data;
}
