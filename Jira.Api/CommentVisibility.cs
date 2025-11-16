using Newtonsoft.Json;

namespace Jira.Api;

/// <summary>
/// Represents the visibility field on a comment.
/// </summary>
public class CommentVisibility
{
	/// <summary>
	/// Create an empty comment visibility object.
	/// </summary>
	public CommentVisibility()
	{
	}

	/// <summary>
	/// Creates a comment visibility object with the given role.
	/// </summary>
	/// <param name="role">The role to apply to the visibility object.</param>
	public CommentVisibility(string role)
	{
		Type = "role";
		Value = role;
	}

	/// <summary>
	/// The type of visibility restriction (e.g., "role" or "group")
	/// </summary>
	[JsonProperty("type")]
	public string Type { get; set; }

	/// <summary>
	/// The value of the visibility restriction (the role or group name)
	/// </summary>
	[JsonProperty("value")]
	public string Value { get; set; }
}
