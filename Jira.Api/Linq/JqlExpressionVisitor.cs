﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Jira.Api.Linq;

public class JqlExpressionVisitor : ExpressionVisitor, IJqlExpressionVisitor
{
	private StringBuilder _jqlWhere;
	private StringBuilder _jqlOrderBy;
	private int? _numberOfResults;
	private int? _skipResults;
	private List<Expression> _whereExpressions;

	public string Jql
	{
		get
		{
			return _jqlWhere.ToString() + _jqlOrderBy.ToString();
		}
	}

	public int? NumberOfResults
	{
		get
		{
			return _numberOfResults;
		}
	}

	public int? SkipResults
	{
		get
		{
			return _skipResults;
		}
	}

	public JqlData Process(Expression expression)
	{
		expression = ExpressionEvaluator.PartialEval(expression);
		_jqlWhere = new StringBuilder();
		_jqlOrderBy = new StringBuilder();
		_whereExpressions = [];
		_numberOfResults = null;
		_skipResults = null;

		Visit(expression);
		return new JqlData { Expression = Jql, NumberOfResults = _numberOfResults, SkipResults = _skipResults };
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
		var operatorString = string.Empty;
		PropertyInfo propertyInfo = null;

		if (value is LiteralMatch)
		{
			// If the right value is a LiteralMatch, ignore the custom attribute.
			operatorString = equal ? JiraOperators.EQUALS : JiraOperators.NOTEQUALS;
		}
		else if (TryGetPropertyInfoFromBinaryExpression(expression, out propertyInfo)
			&& propertyInfo.GetCustomAttributes(typeof(JqlContainsEqualityAttribute), true).Length > 0)
		{
			// Use the equality comparer depending on the presence of custom attribute.
			operatorString = equal ? JiraOperators.CONTAINS : JiraOperators.NOTCONTAINS;
		}
		else
		{
			operatorString = equal ? JiraOperators.EQUALS : JiraOperators.NOTEQUALS;
		}

		_jqlWhere.Append($" {operatorString} ");

		// value
		ProcessConstant(value);
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
		_numberOfResults = int.Parse(((ConstantExpression)node.Arguments[1]).Value.ToString());
	}

	private void ProcessSkip(MethodCallExpression node)
	{
		_skipResults = int.Parse(((ConstantExpression)node.Arguments[1]).Value.ToString());
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

	protected override Expression VisitBinary(BinaryExpression node)
	{
		var isWhere = _whereExpressions.Contains(node);
		var firstWhere = _jqlWhere.Length == 0;
		if (isWhere && !firstWhere)
		{
			_jqlWhere.Append(" and ");
			_whereExpressions.Remove(node);
		}

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

		return node;
	}

}
