using Newtonsoft.Json;

namespace Jira.Api;

/// <summary>
/// Urls for the different renditions of an avatar.
/// </summary>
public class AvatarUrls
{
	/// <summary>
	/// The URL for the 16x16 pixel avatar
	/// </summary>
	[JsonProperty("16x16")]
	public string XSmall { get; set; }

	/// <summary>
	/// The URL for the 24x24 pixel avatar
	/// </summary>
	[JsonProperty("24x24")]
	public string Small { get; set; }

	/// <summary>
	/// The URL for the 32x32 pixel avatar
	/// </summary>
	[JsonProperty("32x32")]
	public string Medium { get; set; }

	/// <summary>
	/// The URL for the 48x48 pixel avatar
	/// </summary>
	[JsonProperty("48x48")]
	public string Large { get; set; }
}
