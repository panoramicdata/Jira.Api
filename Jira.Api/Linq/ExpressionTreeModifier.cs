using System.Linq;
using System.Linq.Expressions;

namespace Jira.Api.Linq;

internal class ExpressionTreeModifier(IQueryable<Issue> queryableIssues) : ExpressionVisitor
{
	private readonly IQueryable<Issue> _queryableIssues = queryableIssues;

	protected override Expression VisitConstant(ConstantExpression node)
	{
		if (node.Type == typeof(JiraQueryable<Issue>))
		{
			return Expression.Constant(_queryableIssues);
		}
		else
		{
			return node;
		}
	}

	protected override Expression VisitMethodCall(MethodCallExpression node)
	{
		if (node.Method.Name == "Where"
			|| node.Method.Name == "Take"
			|| node.Method.Name == "OrderBy"
			|| node.Method.Name == "OrderByDescending"
			|| node.Method.Name == "ThenBy"
			|| node.Method.Name == "ThenByDescending")
		{
			return Expression.Constant(_queryableIssues);
		}

		return base.VisitMethodCall(node);
	}

}
