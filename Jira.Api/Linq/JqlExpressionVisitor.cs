using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Jira.Api.Linq;

/// <summary>
/// Visits LINQ expressions and translates them to JQL (Jira Query Language).
/// </summary>
public partial class JqlExpressionVisitor : ExpressionVisitor, IJqlExpressionVisitor
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
			return $"{_jqlWhere}{_jqlOrderBy}";
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

			foreach (var e in newExpression.Arguments)
			{
				args.Add(((ConstantExpression)e).Value);
			}

			return newExpression.Constructor.Invoke([.. args]);
		}

		throw new NotSupportedException($"Operator '{expression.NodeType}' can only be used with constant values.");
	}
}
