using System.IO;

namespace Jira.Api;

internal class FileSystem : IFileSystem
{
	public byte[] FileReadAllBytes(string path)
	{
		return File.ReadAllBytes(path);
	}
}
