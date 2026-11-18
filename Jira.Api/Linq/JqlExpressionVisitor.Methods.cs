using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Jira.Api.Linq;

public partial class JqlExpressionVisitor
{
	/// <inheritdoc/>
	protected override Expression VisitMethodCall(MethodCallExpression node)
	{
		if (node.Method.Name == "OrderBy"
			|| node.Method.Name == "OrderByDescending"
			|| node.Method.Name == "ThenBy"
			|| node.Method.Name == "ThenByDescending")
		{
			ProcessOrderBy(node);
		}
		else if (node.Method.Name == "Take")
		{
			ProcessTake(node);
		}
		else if (node.Method.Name == "Where")
		{
			ProcessWhere(node);
		}
		else if (node.Method.Name == "Skip")
		{
			ProcessSkip(node);
		}

		return base.VisitMethodCall(node);
	}

	private void ProcessWhere(MethodCallExpression node)
	{
		var member = ((LambdaExpression)((UnaryExpression)node.Arguments[1]).Operand).Body;
		_whereExpressions.Add(member);
	}

	private void ProcessTake(MethodCallExpression node)
	{
		NumberOfResults = int.Parse(((ConstantExpression)node.Arguments[1]).Value.ToString());
	}

	private void ProcessSkip(MethodCallExpression node)
	{
		SkipResults = int.Parse(((ConstantExpression)node.Arguments[1]).Value.ToString());
	}

	private void ProcessOrderBy(MethodCallExpression node)
	{
		var firstOrderBy = _jqlOrderBy.Length == 0;
		var orderByDirection = "asc";
		if (firstOrderBy)
		{
			_jqlOrderBy.Append(" order by ");
		}

		if (node.Method.Name == "OrderByDescending" || node.Method.Name == "ThenByDescending")
		{
			orderByDirection = "desc";
		}

		if (((LambdaExpression)((UnaryExpression)node.Arguments[1]).Operand).Body is MemberExpression member)
		{
			var orderClause = $"{member.Member.Name} {orderByDirection}";

			if (firstOrderBy)
			{
				_jqlOrderBy.Append(orderClause);
			}
			else
			{
				_jqlOrderBy.Insert(10, orderClause + ", ");
			}
		}
	}

	/// <inheritdoc/>
	protected override Expression VisitBinary(BinaryExpression node)
	{
		PrependWhereConjunction(node);
		ProcessBinaryNode(node);
		return node;
	}

	private void PrependWhereConjunction(BinaryExpression node)
	{
		if (_whereExpressions.Contains(node) && _jqlWhere.Length > 0)
		{
			_jqlWhere.Append(" and ");
			_whereExpressions.Remove(node);
		}
	}

	private static readonly Dictionary<ExpressionType, string> _comparisonOperators = new()
	{
		[ExpressionType.GreaterThan] = JiraOperators.GREATERTHAN,
		[ExpressionType.GreaterThanOrEqual] = JiraOperators.GREATERTHANOREQUALS,
		[ExpressionType.LessThan] = JiraOperators.LESSTHAN,
		[ExpressionType.LessThanOrEqual] = JiraOperators.LESSTHANOREQUALS,
	};

	private void ProcessBinaryNode(BinaryExpression node)
	{
		if (_comparisonOperators.TryGetValue(node.NodeType, out var comparisonOperator))
		{
			ProcessGreaterAndLessThanOperator(node, comparisonOperator);
			return;
		}

		switch (node.NodeType)
		{
			case ExpressionType.Equal:
				ProcessEqualityOperator(node, true);
				break;

			case ExpressionType.NotEqual:
				ProcessEqualityOperator(node, false);
				break;

			case ExpressionType.AndAlso:
				ProcessUnionOperator(node, JiraOperators.AND);
				break;

			case ExpressionType.OrElse:
				ProcessUnionOperator(node, JiraOperators.OR);
				break;

			default:
				throw new NotSupportedException($"Expression type '{node.NodeType}' is not supported.");
		}
	}
}
