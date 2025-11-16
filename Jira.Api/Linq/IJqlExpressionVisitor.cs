namespace Jira.Api.Linq;

/// <summary>
/// Abstracts the translation of an Expression tree into JQL
/// </summary>
public interface IJqlExpressionVisitor
{
	/// <summary>
	/// Processes an expression tree and converts it to JQL data
	/// </summary>
	/// <param name="expression">The expression to process</param>
	/// <returns>The JQL data</returns>
	JqlData Process(System.Linq.Expressions.Expression expression);
}
