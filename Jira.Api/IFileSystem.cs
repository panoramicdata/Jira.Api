namespace Jira.Api;

public interface IFileSystem
{
	byte[] FileReadAllBytes(string path);
}
