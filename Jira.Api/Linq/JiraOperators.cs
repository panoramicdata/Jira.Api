namespace Jira.Api.Linq;

/// <summary>
/// Container for the supported JIRA operator strings.
/// </summary>
public class JiraOperators
{
	/// <summary>
	/// Equality operator
	/// </summary>
	public const string EQUALS = "=";

	/// <summary>
	/// Inequality operator
	/// </summary>
	public const string NOTEQUALS = "!=";

	/// <summary>
	/// Contains operator
	/// </summary>
	public const string CONTAINS = "~";

	/// <summary>
	/// Not contains operator
	/// </summary>
	public const string NOTCONTAINS = "!~";

	/// <summary>
	/// Is operator
	/// </summary>
	public const string IS = "is";

	/// <summary>
	/// Is not operator
	/// </summary>
	public const string ISNOT = "is not";

	/// <summary>
	/// Greater than operator
	/// </summary>
	public const string GREATERTHAN = ">";

	/// <summary>
	/// Less than operator
	/// </summary>
	public const string LESSTHAN = "<";

	/// <summary>
	/// Greater than or equals operator
	/// </summary>
	public const string GREATERTHANOREQUALS = ">=";

	/// <summary>
	/// Less than or equals operator
	/// </summary>
	public const string LESSTHANOREQUALS = "<=";

	/// <summary>
	/// Logical OR operator
	/// </summary>
	public const string OR = "or";

	/// <summary>
	/// Logical AND operator
	/// </summary>
	public const string AND = "and";
}
