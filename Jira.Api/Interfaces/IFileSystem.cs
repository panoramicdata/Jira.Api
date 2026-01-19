namespace Jira.Api.Interfaces;

/// <summary>
/// Interface for file system operations.
/// </summary>
public interface IFileSystem
{
	/// <summary>
	/// Reads all bytes from a file.
	/// </summary>
	byte[] FileReadAllBytes(string path);
}
