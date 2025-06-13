namespace Jira.Api;

/// <summary>
/// Information about an attachment to be uploaded
/// </summary>
public class UploadAttachmentInfo(string name, byte[] data)
{
	public string Name { get; set; } = name;

	public byte[] Data { get; set; } = data;
}
