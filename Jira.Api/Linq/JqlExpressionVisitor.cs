using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Jira.Api.Linq;

/// <summary>
/// Visits LINQ expressions and translates them to JQL (Jira Query Language).
/// </summary>
public class JqlExpressionVisitor : ExpressionVisitor, IJqlExpressionVisitor
{
	private StringBuilder _jqlWhere;
	private StringBuilder _jqlOrderBy;
	private List<Expression> _whereExpressions;

	/// <summary>
	/// Gets the generated JQL query string.
	/// </summary>
	public string Jql
	{
		get
		{
			return _jqlWhere.ToString() + _jqlOrderBy.ToString();
		}
	}

	/// <summary>
	/// Gets the maximum number of results to return, or <see langword="null"/> if not specified.
	/// </summary>
	public int? NumberOfResults { get; private set; }

	/// <summary>
	/// Gets the number of results to skip, or <see langword="null"/> if not specified.
	/// </summary>
	public int? SkipResults { get; private set; }

	/// <summary>
	/// Processes the specified expression and generates JQL data.
	/// </summary>
	/// <param name="expression">The LINQ expression to process.</param>
	/// <returns>A <see cref="JqlData"/> object containing the generated JQL query and pagination information.</returns>
	public JqlData Process(Expression expression)
	{
		expression = ExpressionEvaluator.PartialEval(expression);
		_jqlWhere = new StringBuilder();
		_jqlOrderBy = new StringBuilder();
		_whereExpressions = [];
		NumberOfResults = null;
		SkipResults = null;

		Visit(expression);
		return new JqlData { Expression = Jql, NumberOfResults = NumberOfResults, SkipResults = SkipResults };
	}

	private static string GetFieldNameFromBinaryExpression(BinaryExpression expression)
	{
		if (TryGetPropertyInfoFromBinaryExpression(expression, out PropertyInfo propertyInfo))
		{
			var attributes = propertyInfo.GetCustomAttributes(typeof(JqlFieldNameAttribute), true);
			if (attributes.Length > 0)
			{
				return ((JqlFieldNameAttribute)attributes[0]).Name;
			}
			else
			{
				return propertyInfo.Name;
			}
		}

		if (expression.Left is MethodCallExpression methodCallExpression)
		{
			return $"\"{((ConstantExpression)methodCallExpression.Arguments[0]).Value}\"";
		}

		throw new NotSupportedException($"Operator '{expression.NodeType}' can only be applied on the right side of properties and property indexers.");
	}

	private static bool TryGetPropertyInfoFromBinaryExpression(BinaryExpression expression, out PropertyInfo propertyInfo)
	{
		var memberExpression = expression.Left as MemberExpression;
		if (expression.Left is UnaryExpression unaryExpression)
		{
			memberExpression = unaryExpression.Operand as MemberExpression;
		}

		if (memberExpression != null)
		{
			propertyInfo = memberExpression.Member as PropertyInfo;
			if (propertyInfo != null)
			{
				return true;
			}
		}

		propertyInfo = null;
		return false;
	}

	private static object GetFieldValueFromBinaryExpression(BinaryExpression expression)
	{
		if (expression.Right.NodeType == ExpressionType.Constant)
		{
			return ((ConstantExpression)expression.Right).Value;
		}
		else if (expression.Right.NodeType == ExpressionType.New)
		{
			var newExpression = (NewExpression)expression.Right;
			var args = new List<object>();

			foreach (ConstantExpression e in newExpression.Arguments)
			{
				args.Add(e.Value);
			}

			return newExpression.Constructor.Invoke([.. args]);
		}

		throw new NotSupportedException($"Operator '{expression.NodeType}' can only be used with constant values.");
	}

	private void ProcessGreaterAndLessThanOperator(BinaryExpression expression, string operatorString)
	{
		var fieldName = GetFieldNameFromBinaryExpression(expression);
		var value = GetFieldValueFromBinaryExpression(expression);

		// field
		_jqlWhere.Append(fieldName);

		// operator
		_jqlWhere.Append($" {operatorString} ");

		// value
		ProcessConstant(value);
	}

	private void ProcessEqualityOperator(BinaryExpression expression, bool equal)
	{
		if (expression.Left is MemberExpression || expression.Left is UnaryExpression)
		{
			ProcessMemberEqualityOperator(expression, equal);
		}
		else if (expression.Left is MethodCallExpression)
		{
			ProcessIndexedMemberEqualityOperator(expression, equal);
		}
	}

	private void ProcessIndexedMemberEqualityOperator(BinaryExpression expression, bool equal)
	{
		var fieldName = GetFieldNameFromBinaryExpression(expression);
		var fieldValue = GetFieldValueFromBinaryExpression(expression);

		// field
		_jqlWhere.Append(fieldName);

		// operator
		string operatorString;
		if (fieldValue is string)
		{
			operatorString = equal ? JiraOperators.CONTAINS : JiraOperators.NOTCONTAINS;
		}
		else
		{
			operatorString = equal ? JiraOperators.EQUALS : JiraOperators.NOTEQUALS;
		}

		_jqlWhere.Append($" {operatorString} ");

		// value
		ProcessConstant(fieldValue);
	}

	private void ProcessMemberEqualityOperator(BinaryExpression expression, bool equal)
	{
		var field = GetFieldNameFromBinaryExpression(expression);
		var value = GetFieldValueFromBinaryExpression(expression);

		// field
		_jqlWhere.Append(field);

		// special cases for empty/null string
		if (value == null || value.Equals(""))
		{
			_jqlWhere.Append(' ');
			_jqlWhere.Append(equal ? JiraOperators.IS : JiraOperators.ISNOT);
			_jqlWhere.Append(' ');
			_jqlWhere.Append(value == null ? "null" : "empty");
			return;
		}

		// operator
		var operatorString = GetEqualityOperator(expression, value, equal);

		_jqlWhere.Append($" {operatorString} ");

		// value
		ProcessConstant(value);
	}

	private static string GetEqualityOperator(BinaryExpression expression, object value, bool equal)
	{
		if (value is LiteralMatch)
		{
			return equal ? JiraOperators.EQUALS : JiraOperators.NOTEQUALS;
		}

		if (TryGetPropertyInfoFromBinaryExpression(expression, out var propertyInfo)
			&& propertyInfo.GetCustomAttributes(typeof(JqlContainsEqualityAttribute), true).Length > 0)
		{
			return equal ? JiraOperators.CONTAINS : JiraOperators.NOTCONTAINS;
		}

		return equal ? JiraOperators.EQUALS : JiraOperators.NOTEQUALS;
	}

	private void ProcessConstant(object value)
	{
		var valueType = value.GetType();
		if (valueType == typeof(string)
			|| valueType == typeof(ComparableString)
			|| valueType == typeof(LiteralDateTime)
			|| valueType == typeof(LiteralMatch))
		{
			_jqlWhere.Append($"\"{value}\"");
		}
		else if (valueType == typeof(DateTime))
		{
			_jqlWhere.Append($"\"{JiraClient.FormatDateTimeString((DateTime)value)}\"");
		}
		else
		{
			_jqlWhere.Append(value);
		}
	}

	private void ProcessUnionOperator(BinaryExpression expression, string operatorString)
	{
		_jqlWhere.Append('(');
		Visit(expression.Left);
		_jqlWhere.Append(" " + operatorString + " ");
		Visit(expression.Right);
		_jqlWhere.Append(')');
	}

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

	private void ProcessBinaryNode(BinaryExpression node)
	{
		switch (node.NodeType)
		{
			case ExpressionType.GreaterThan:
				ProcessGreaterAndLessThanOperator(node, JiraOperators.GREATERTHAN);
				break;

			case ExpressionType.GreaterThanOrEqual:
				ProcessGreaterAndLessThanOperator(node, JiraOperators.GREATERTHANOREQUALS);
				break;

			case ExpressionType.LessThan:
				ProcessGreaterAndLessThanOperator(node, JiraOperators.LESSTHAN);
				break;

			case ExpressionType.LessThanOrEqual:
				ProcessGreaterAndLessThanOperator(node, JiraOperators.LESSTHANOREQUALS);
				break;

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
