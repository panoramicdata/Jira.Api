﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Jira.Api.Linq;

/// <summary>
/// Evaluates subtrees that contain local variables.
/// </summary>
/// <remarks>
/// Thanks to http://blogs.msdn.com/b/mattwar/archive/2007/08/01/linq-building-an-iqueryable-provider-part-iii.aspx
/// for providing the source for this class
/// </remarks>
internal static class ExpressionEvaluator
{
	/// <summary>
	/// Performs evaluation & replacement of independent sub-trees
	/// </summary>
	/// <param name="expression">The root of the expression tree.</param>
	/// <returns>A new tree with sub-trees evaluated and replaced.</returns>
	public static Expression PartialEval(Expression expression)
	{
		return PartialEval(expression, ExpressionEvaluator.CanBeEvaluatedLocally);
	}

	/// <summary>
	/// Performs evaluation & replacement of independent sub-trees
	/// </summary>
	/// <param name="expression">The root of the expression tree.</param>
	/// <param name="fnCanBeEvaluated">A function that decides whether a given expression node can be part of the local function.</param>
	/// <returns>A new tree with sub-trees evaluated and replaced.</returns>
	public static Expression PartialEval(Expression expression, Func<Expression, bool> fnCanBeEvaluated)
	{
		return new SubtreeEvaluator(new Nominator(fnCanBeEvaluated).Nominate(expression)).Eval(expression);
	}

	private static bool CanBeEvaluatedLocally(Expression expression)
	{
		return expression.NodeType != ExpressionType.Parameter;
	}

	/// <summary>
	/// Evaluates & replaces sub-trees when first candidate is reached (top-down)
	/// </summary>
	class SubtreeEvaluator : ExpressionVisitor
	{
		HashSet<Expression> candidates;

		internal SubtreeEvaluator(HashSet<Expression> candidates)
		{
			this.candidates = candidates;
		}

		internal Expression Eval(Expression exp)
		{
			return Visit(exp);
		}

		public override Expression Visit(Expression exp)
		{
			if (exp == null)
			{
				return null;
			}

			if (candidates.Contains(exp))
			{
				return Evaluate(exp);
			}

			return base.Visit(exp);
		}

		private static Expression Evaluate(Expression e)
		{
			if (e.NodeType == ExpressionType.Constant)
			{
				return e;
			}

			LambdaExpression lambda = Expression.Lambda(e);
			Delegate fn = lambda.Compile();
			return Expression.Constant(fn.DynamicInvoke(null), e.Type);
		}
	}

	/// <summary>
	/// Performs bottom-up analysis to determine which nodes can possibly
	/// be part of an evaluated sub-tree.
	/// </summary>
	class Nominator : ExpressionVisitor
	{
		Func<Expression, bool> fnCanBeEvaluated;
		HashSet<Expression> candidates;
		bool cannotBeEvaluated;

		internal Nominator(Func<Expression, bool> fnCanBeEvaluated)
		{
			this.fnCanBeEvaluated = fnCanBeEvaluated;
		}

		internal HashSet<Expression> Nominate(Expression expression)
		{
			candidates = [];
			Visit(expression);
			return candidates;
		}

		public override Expression Visit(Expression expression)
		{
			if (expression != null)
			{
				bool saveCannotBeEvaluated = cannotBeEvaluated;
				cannotBeEvaluated = false;
				base.Visit(expression);
				if (!cannotBeEvaluated)
				{
					if (fnCanBeEvaluated(expression))
					{
						candidates.Add(expression);
					}
					else
					{
						cannotBeEvaluated = true;
					}
				}

				cannotBeEvaluated |= saveCannotBeEvaluated;
			}

			return expression;
		}
	}
}
