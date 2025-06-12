using System;
using System.Collections.Generic;
using System.Linq;
using RestSharp;

namespace Jira.Api;

public class QueryParametersHelper
{
	/// <summary>
	/// Gets the parameters from a full query string.
	/// </summary>
	/// <param name="query">The url query.</param>
	/// <returns>List of all parameters within the query.</returns>
	[Obsolete("GetQueryParametersFromPath() use obsolete RestSharp.Parameter. Use GetParametersFromPath() instead.")]
	public static IEnumerable<RestSharp.Parameter> GetQueryParametersFromPath(string query)
	{
		return GetParametersFromPath(query).Select(x => new RestSharp.Parameter(x.Name, x.Value, x.Type, x.Encode));
	}

	/// <summary>
	/// Gets the parameters from a full query string.
	/// </summary>
	/// <param name="query">The url query.</param>
	/// <returns>List of all parameters within the query.</returns>
	public static IEnumerable<Parameter> GetParametersFromPath(string query)
	{
		var parameters = query.TrimStart('?')
			.Split(['&'], StringSplitOptions.RemoveEmptyEntries)
			.Select(s =>
			{
				var p = s.Split(['='], 2);
				return new Parameter(name: p[0], value: p.Length > 1 ? p[1] : "", type: ParameterType.QueryString);
			});

		return parameters;
	}

	public class Parameter(string name, object value, ParameterType type, bool encode = true)
	{
		public string Name { get; } = name;

		public object Value { get; } = value;

		public ParameterType Type { get; } = type;

		public bool Encode { get; } = encode;
	}
}
