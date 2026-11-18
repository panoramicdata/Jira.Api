namespace Jira.Api.Models;

/// <summary>
/// Default workflow actions for a standard JIRA install.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S2339:Public constant members should not be used", Justification = "Published public API: changing a public const to a static read-only member is a breaking change for existing consumers.")]
public static class WorkflowActions
{
	/// <summary>
	/// Resolve Issue action
	/// </summary>
	public const string Resolve = "Resolve Issue";

	/// <summary>
	/// Close Issue action
	/// </summary>
	public const string Close = "Close Issue";

	/// <summary>
	/// Start Progress action
	/// </summary>
	public const string StartProgress = "Start Progress";
}
