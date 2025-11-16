using System;

namespace Jira.Api;

/// <summary>
/// Type that wraps a string and exposes operator overloads for
/// easier LINQ queries
/// </summary>
/// <remarks>
/// Allows comparisons in the form of issue.Key > "TST-1"
/// </remarks>
public class ComparableString(string value)
{
	/// <summary>
	/// The wrapped string value
	/// </summary>
	public string Value { get; set; } = value;

	/// <summary>
	/// Implicitly converts a string to a ComparableString
	/// </summary>
	public static implicit operator ComparableString(string value)
	{
		if (value != null)
		{
			return new ComparableString(value);
		}
		else
		{
			return null;
		}
	}

	/// <summary>
	/// Equality operator for comparing with a string
	/// </summary>
	public static bool operator ==(ComparableString field, string value)
	{
		if (field is null)
		{
			return value == null;
		}
		else
		{
			return field.Value == value;
		}
	}

	/// <summary>
	/// Inequality operator for comparing with a string
	/// </summary>
	public static bool operator !=(ComparableString field, string value)
	{
		if (field is null)
		{
			return value != null;
		}
		else
		{
			return field.Value != value;
		}
	}

	/// <summary>
	/// Greater than operator for comparing with a string
	/// </summary>
	public static bool operator >(ComparableString field, string value)
	{
		return field.Value.CompareTo(value) > 0;
	}

	/// <summary>
	/// Less than operator for comparing with a string
	/// </summary>
	public static bool operator <(ComparableString field, string value)
	{
		return field.Value.CompareTo(value) < 0;
	}

	/// <summary>
	/// Less than or equal operator for comparing with a string
	/// </summary>
	public static bool operator <=(ComparableString field, string value)
	{
		return field.Value.CompareTo(value) <= 0;
	}

	/// <summary>
	/// Greater than or equal operator for comparing with a string
	/// </summary>
	public static bool operator >=(ComparableString field, string value)
	{
		return field.Value.CompareTo(value) >= 0;
	}

	/// <summary>
	/// Equality operator for comparing with a DateTime
	/// </summary>
	public static bool operator ==(ComparableString field, DateTime value)
	{
		if (field is null)
		{
			return value == null;
		}
		else
		{
			return field.Value == JiraClient.FormatDateTimeString(value);
		}
	}

	/// <summary>
	/// Inequality operator for comparing with a DateTime
	/// </summary>
	public static bool operator !=(ComparableString field, DateTime value)
	{
		if (field is null)
		{
			return value != null;
		}
		else
		{
			return field.Value != JiraClient.FormatDateTimeString(value);
		}
	}

	/// <summary>
	/// Greater than operator for comparing with a DateTime
	/// </summary>
	public static bool operator >(ComparableString field, DateTime value)
	{
		return field.Value.CompareTo(JiraClient.FormatDateTimeString(value)) > 0;
	}

	/// <summary>
	/// Less than operator for comparing with a DateTime
	/// </summary>
	public static bool operator <(ComparableString field, DateTime value)
	{
		return field.Value.CompareTo(JiraClient.FormatDateTimeString(value)) < 0;
	}

	/// <summary>
	/// Less than or equal operator for comparing with a DateTime
	/// </summary>
	public static bool operator <=(ComparableString field, DateTime value)
	{
		return field.Value.CompareTo(JiraClient.FormatDateTimeString(value)) <= 0;
	}

	/// <summary>
	/// Greater than or equal operator for comparing with a DateTime
	/// </summary>
	public static bool operator >=(ComparableString field, DateTime value)
	{
		return field.Value.CompareTo(JiraClient.FormatDateTimeString(value)) >= 0;
	}

	/// <summary>
	/// Returns the string representation of this instance
	/// </summary>
	public override string ToString()
	{
		return Value;
	}

	/// <summary>
	/// Determines whether the specified object is equal to this instance
	/// </summary>
	public override bool Equals(object obj)
	{
		if (obj is ComparableString comparableString)
		{
			return Value.Equals(comparableString.Value);
		}
		else if (obj is string obString)
		{
			return Value.Equals(obString);
		}

		return base.Equals(obj);
	}

	/// <summary>
	/// Returns the hash code for this instance
	/// </summary>
	public override int GetHashCode()
	{
		if (Value == null)
		{
			return 0;
		}

		return Value.GetHashCode();
	}
}
