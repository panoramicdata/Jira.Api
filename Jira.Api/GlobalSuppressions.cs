using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Major Code Smell", "S2360:Optional parameters should not be used", Scope = "namespaceanddescendants", Target = "~N:Jira.Api.Interfaces", Justification = "Published public API: replacing optional parameters on the service interfaces with overloads is a breaking change for existing consumers and implementers.")]
