using System;

namespace Jira.Api.Linq;

/// <summary>
/// Attribute that can be applied to properties to use a "Contains" rather than "Equals"
/// when performing equality comparisons.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
internal class JqlContainsEqualityAttribute : System.Attribute
{
}
