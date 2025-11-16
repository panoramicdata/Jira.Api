namespace Jira.Api;

/// <summary>
/// Interface for file system operations
/// </summary>
public interface IFileSystem
{
	/// <summary>
	/// Reads all bytes from a file
	/// </summary>
	/// <param name="path">The file path to read from</param>
	/// <returns>The bytes read from the file</returns>
	byte[] FileReadAllBytes(string path);
}
