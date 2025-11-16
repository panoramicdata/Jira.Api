namespace Jira.Api.Linq;

/// <summary>
/// JQL query data
/// </summary>
public class JqlData
{
	/// <summary>
	/// The JQL expression
	/// </summary>
	public string Expression { get; set; }

	/// <summary>
	/// Maximum number of results to return
	/// </summary>
	public int? NumberOfResults { get; set; }

	/// <summary>
	/// Number of results to skip
	/// </summary>
	public int? SkipResults { get; set; }
}
