using System;
using System.Diagnostics.CodeAnalysis;

namespace Jira.Api;

/// <summary>
/// Force a DateTime field to use a string provided as the JQL query value.
/// </summary>
[SuppressMessage("N/A", "CS0660", Justification = "Operator overloads are used for LINQ to JQL provider.")]
[SuppressMessage("N/A", "CS0661", Justification = "Operator overloads are used for LINQ to JQL provider.")]
public class LiteralDateTime(string dateTimeString)
{
	private readonly string _dateTimeString = dateTimeString;

	/// <summary>
	/// Returns the string representation
	/// </summary>
	public override string ToString()
	{
		return _dateTimeString;
	}

	/// <summary>
	/// Equality operator for DateTime comparison
	/// </summary>
	public static bool operator ==(DateTime dateTime, LiteralDateTime literalDateTime)
	{
		return false;
	}

	/// <summary>
	/// Inequality operator for DateTime comparison
	/// </summary>
	public static bool operator !=(DateTime dateTime, LiteralDateTime literalDateTime)
	{
		return false;
	}

	/// <summary>
	/// Greater than operator for DateTime comparison
	/// </summary>
	public static bool operator >(DateTime dateTime, LiteralDateTime literalDateTime)
	{
		return false;
	}

	/// <summary>
	/// Less than operator for DateTime comparison
	/// </summary>
	public static bool operator <(DateTime dateTime, LiteralDateTime literalDateTime)
	{
		return false;
	}

	/// <summary>
	/// Greater than or equal operator for DateTime comparison
	/// </summary>
	public static bool operator >=(DateTime dateTime, LiteralDateTime literalDateTime)
	{
		return false;
	}

	/// <summary>
	/// Less than or equal operator for DateTime comparison
	/// </summary>
	public static bool operator <=(DateTime dateTime, LiteralDateTime literalDateTime)
	{
		return false;
	}

	/// <summary>
	/// Equality operator for nullable DateTime comparison
	/// </summary>
	public static bool operator ==(DateTime? dateTime, LiteralDateTime literalDateTime)
	{
		return false;
	}

	/// <summary>
	/// Inequality operator for nullable DateTime comparison
	/// </summary>
	public static bool operator !=(DateTime? dateTime, LiteralDateTime literalDateTime)
	{
		return false;
	}

	/// <summary>
	/// Greater than operator for nullable DateTime comparison
	/// </summary>
	public static bool operator >(DateTime? dateTime, LiteralDateTime literalDateTime)
	{
		return false;
	}

	/// <summary>
	/// Less than operator for nullable DateTime comparison
	/// </summary>
	public static bool operator <(DateTime? dateTime, LiteralDateTime literalDateTime)
	{
		return false;
	}

	/// <summary>
	/// Greater than or equal operator for nullable DateTime comparison
	/// </summary>
	public static bool operator >=(DateTime? dateTime, LiteralDateTime literalDateTime)
	{
		return false;
	}

	/// <summary>
	/// Less than or equal operator for nullable DateTime comparison
	/// </summary>
	public static bool operator <=(DateTime? dateTime, LiteralDateTime literalDateTime)
	{
		return false;
	}
}
