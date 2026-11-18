using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Jira.Api.Linq;

public partial class JqlExpressionVisitor
{
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
}
