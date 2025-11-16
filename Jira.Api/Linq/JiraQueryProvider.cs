using System;
using System.Linq;
using System.Linq.Expressions;

namespace Jira.Api.Linq;

/// <summary>
/// Query provider for JIRA issue queries
/// </summary>
public class JiraQueryProvider(IJqlExpressionVisitor translator, IIssueService issues) : IQueryProvider
{
	private readonly IJqlExpressionVisitor _translator = translator;
	private readonly IIssueService _issues = issues;

	/// <summary>
	/// Creates a queryable instance
	/// </summary>
	public IQueryable<T> CreateQuery<T>(Expression expression)
	{
		return new JiraQueryable<T>(this, expression);
	}

	/// <summary>
	/// Creates a queryable instance
	/// </summary>
	public IQueryable CreateQuery(Expression expression)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Executes the query
	/// </summary>
	public T Execute<T>(Expression expression)
	{
		bool isEnumerable = (typeof(T).Name == "IEnumerable`1");

		return (T)Execute(expression, isEnumerable);
	}

	/// <summary>
	/// Executes the query
	/// </summary>
	public object Execute(Expression expression)
	{
		return Execute(expression, true);
	}

	private object Execute(Expression expression, bool isEnumerable)
	{
		var jql = _translator.Process(expression);

		var temp = _issues.GetIssuesFromJqlAsync(jql.Expression, jql.SkipResults ?? 0, jql.NumberOfResults, default).GetAwaiter().GetResult();
		IQueryable<Issue> issues = temp.AsQueryable();

		if (isEnumerable)
		{
			return issues;
		}
		else
		{
			var treeCopier = new ExpressionTreeModifier(issues);
			Expression newExpressionTree = treeCopier.Visit(expression);

			return issues.Provider.Execute(newExpressionTree);
		}
	}
}
