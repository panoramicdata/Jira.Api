using Newtonsoft.Json;

namespace Jira.Api;

/// <summary>
/// Class that encapsulates the necessary information to create a new jira user.
/// </summary>
public class JiraUserCreationInfo
{
	/// <summary>
	/// The username
	/// </summary>
	[JsonProperty("name")]
	public string Username { get; set; }

	/// <summary>
	/// The display name
	/// </summary>
	[JsonProperty("displayName")]
	public string DisplayName { get; set; }

	/// <summary>
	/// The email address
	/// </summary>
	[JsonProperty("emailAddress")]
	public string Email { get; set; }

	/// <summary>
	/// The password (if not set, password will be randomly generated)
	/// </summary>
	[JsonProperty("password")]
	public string Password { get; set; }

	/// <summary>
	/// Set to true to have the user notified by email upon account creation. False to prevent notification.
	/// </summary>
	[JsonProperty("notification")]
	public bool Notification { get; set; }

	/// <summary>
	/// Returns the string representation of this user creation info
	/// </summary>
	public override string ToString()
	{
		return Username;
	}
}
