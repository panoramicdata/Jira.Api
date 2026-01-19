using System.IO;

namespace Jira.Api.Services;

internal class FileSystem : IFileSystem
{
	public byte[] FileReadAllBytes(string path)
	{
		return File.ReadAllBytes(path);
	}
}
