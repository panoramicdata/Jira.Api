using System;
using System.Linq;
using System.Linq.Expressions;

namespace Jira.Api.Linq;

public class JiraQueryProvider(IJqlExpressionVisitor translator, IIssueService issues) : IQueryProvider
{
	private readonly IJqlExpressionVisitor _translator = translator;
	private readonly IIssueService _issues = issues;

	public IQueryable<T> CreateQuery<T>(Expression expression)
	{
		return new JiraQueryable<T>(this, expression);
	}

	public IQueryable CreateQuery(Expression expression)
	{
		throw new NotImplementedException();
	}

	public T Execute<T>(Expression expression)
	{
		bool isEnumerable = (typeof(T).Name == "IEnumerable`1");

		return (T)this.Execute(expression, isEnumerable);
	}

	public object Execute(Expression expression)
	{
		return Execute(expression, true);
	}

	private object Execute(Expression expression, bool isEnumerable)
	{
		var jql = _translator.Process(expression);

		var temp = _issues.GetIssuesFromJqlAsync(jql.Expression, jql.NumberOfResults, jql.SkipResults ?? 0).Result;
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
