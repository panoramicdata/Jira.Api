using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Jira.Api.Linq;

/// <summary>
/// Queryable implementation for JIRA issues
/// </summary>
/// <typeparam name="T">The type being queried</typeparam>
public class JiraQueryable<T> : IOrderedQueryable<T>, IQueryable<T>
{
	private readonly JiraQueryProvider _provider;
	private readonly Expression _expression;

	/// <summary>
	/// Initializes a new instance of the JiraQueryable class
	/// </summary>
	public JiraQueryable(JiraQueryProvider provider)
	{
		_provider = provider;
		_expression = Expression.Constant(this);
	}

	/// <summary>
	/// Initializes a new instance of the JiraQueryable class with an expression
	/// </summary>
	public JiraQueryable(JiraQueryProvider provider, Expression expression)
	{
		_provider = provider;
		_expression = expression;
	}

	/// <summary>
	/// Returns an enumerator that iterates through the collection
	/// </summary>
	public IEnumerator<T> GetEnumerator()
	{
		return ((IEnumerable<T>)_provider.Execute(Expression)).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)_provider.Execute(Expression)).GetEnumerator();
	}

	/// <summary>
	/// The type of the elements in the query
	/// </summary>
	public Type ElementType
	{
		get
		{
			return typeof(T);
		}
	}

	/// <summary>
	/// The expression tree associated with this queryable
	/// </summary>
	public Expression Expression
	{
		get
		{
			return _expression;
		}
	}

	/// <summary>
	/// The query provider that executes the query
	/// </summary>
	public IQueryProvider Provider
	{
		get
		{
			return _provider;
		}
	}
}
