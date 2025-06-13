using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jira.Api;

/// <summary>
/// Provides utility methods for working with query parameters in URL strings.
/// </summary>
/// <remarks>This class includes methods for extracting and manipulating query parameters from URL strings. It is
/// designed to simplify handling query strings in HTTP requests or similar operations.</remarks>
public class QueryParametersHelper
{
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

	/// <summary>
	/// Represents a parameter used in a request, including its name, value, type, and encoding behavior.
	/// </summary>
	/// <remarks>This class is typically used to define parameters for HTTP requests or similar operations. The <see
	/// cref="Encode"/> property determines whether the parameter value should be URL-encoded.</remarks>
	/// <param name="name"></param>
	/// <param name="value"></param>
	/// <param name="type"></param>
	/// <param name="encode"></param>
	public class Parameter(string name, object value, ParameterType type, bool encode = true)
	{
		/// <summary>
		/// Gets the name associated with the current instance.
		/// </summary>
		public string Name { get; } = name;

		/// <summary>
		/// Gets the value associated with the current instance.
		/// </summary>
		public object Value { get; } = value;

		/// <summary>
		/// The type of the parameter, indicating how it should be used in a request.
		/// </summary>
		public ParameterType Type { get; } = type;

		/// <summary>
		/// Whether to encode the value when sent in a request.
		///	</summary>
		public bool Encode { get; } = encode;
	}
}
