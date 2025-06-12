using Newtonsoft.Json;

namespace Jira.Api;

/// <summary>
/// Class that encapsulates the necessary information to create a new project component.
/// </summary>
/// <remarks>
/// Creates a new instance of ProjectComponentCreationInfo.
/// </remarks>
/// <param name="name">The name of the project component.</param>
public class ProjectComponentCreationInfo(string name)
{

	/// <summary>
	/// Name of the project component.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; set; } = name;

	/// <summary>
	/// Description of the project component.
	/// </summary>
	[JsonProperty("description")]
	public string Description { get; set; }

	/// <summary>
	/// Key of the project to associate with this component.
	/// </summary>
	[JsonProperty("project")]
	public string ProjectKey { get; set; }
}
