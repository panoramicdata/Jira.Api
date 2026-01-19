namespace Jira.Api.Models;

/// <summary>
/// Represents a screen in Jira.
/// </summary>
public class Screen
{
	/// <summary>
	/// Creates an instance from the remote entity.
	/// </summary>
	internal Screen(RemoteScreen remote)
	{
		Id = remote.Id;
		Name = remote.Name;
		Description = remote.Description;
	}

	/// <summary>
	/// The screen ID.
	/// </summary>
	public long Id { get; }

	/// <summary>
	/// The screen name.
	/// </summary>
	public string? Name { get; }

	/// <summary>
	/// The screen description.
	/// </summary>
	public string? Description { get; }

	/// <summary>
	/// Returns a string representation of this screen.
	/// </summary>
	public override string ToString() => Name ?? $"Screen {Id}";
}
