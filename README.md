# Jira.Api nuget package for .NET developers
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/f51368d841ca4d4b9fb142bb72d55b98)](https://app.codacy.com/gh/panoramicdata/Jira.Api/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)
[![NuGet](https://img.shields.io/nuget/v/Jira.Api.svg)](https://www.nuget.org/packages/Jira.Api/)
[![License: BSD](https://img.shields.io/badge/License-BSD-blue.svg)](https://opensource.org/licenses/BSD-3-Clause)

Contains utilities for interacting with  [Atlassian JIRA](http://www.atlassian.com/software/jira) in .NET (C# etc.).

## Support Notice

All features tested on JIRA v9.12.2

## ?? BREAKING CHANGES

### Latest Breaking Changes

**Namespace Reorganization:**
* All interfaces have been moved to `Jira.Api.Interfaces` namespace
* All models have been moved to `Jira.Api.Models` namespace
* You will need to add `using Jira.Api.Interfaces;` and `using Jira.Api.Models;` to your code

**New Services Added:**
* `IWorkflowService` - Query workflows
* `IWorkflowSchemeService` - Full CRUD operations for workflow schemes
* `IProjectStatusService` - Query project statuses by issue type

### Previous Breaking Changes (from Atlassian.SDK)

If you are migrating from Atlassian.SDK, there are MANY breaking changes, we believe all for the better.

The main items:

* The namespace has changed from `Atlassian.SDK` to `Jira.Api`.
* .NET 9.0+ only.  What is this, 2024?
* EVERYTHING is asynchronous now, no more sync methods hiding async under the covers.
* Methods still have the same names, but they are now all suffixed with `Async` e.g. UpdateIssueAsync().
* You MUST provide cancellation tokens to all async methods.
* There are no more optional parameters, all parameters are required.
* The order of skip and take parameters has been changed for everyone's sanity.
	- The new order is `skip, take` instead of `take, skip`.
	- This is a nasty one if you are already specifying these value.
* Attachment uploads use FileInfo instead of full path file names.
* The later versions of RestSharp use (for example) Method.Post instead of Method.POST, so you will need to update your code if you are using RestSharp directly.

Don't like these changes? Here are your options:

* Use the old SDK, it is still available as `Atlassian.SDK`.
* Ask Atlassian to make an "official" nuget that meets your requirements.
* Submit your Pull Requests, we are happy to consider them.
* Fork off ... this project and make your own changes.

Caveats - this project is still very new.  We may make further breaking changes in the future, but we will try to avoid them.

## New Features

### Workflow Management

```csharp
// Get all workflows
var workflows = await jiraClient.Workflows.GetWorkflowsAsync();

// Get a specific workflow by name  
var workflow = await jiraClient.Workflows.GetWorkflowAsync("My Workflow");
```

### Workflow Scheme Management (Full CRUD)

```csharp
// Get all workflow schemes (paginated)
var schemes = await jiraClient.WorkflowSchemes.GetWorkflowSchemesAsync(startAt: 0, maxResults: 50);

// Get a workflow scheme by ID
var scheme = await jiraClient.WorkflowSchemes.GetWorkflowSchemeAsync("10001");

// Get the workflow scheme for a project
var projectScheme = await jiraClient.WorkflowSchemes.GetWorkflowSchemeForProjectAsync("PROJ");

// Create a new workflow scheme
var newScheme = await jiraClient.WorkflowSchemes.CreateWorkflowSchemeAsync(
    name: "My Workflow Scheme",
    description: "A custom workflow scheme",
    defaultWorkflow: "jira");

// Update a workflow scheme
var updatedScheme = await jiraClient.WorkflowSchemes.UpdateWorkflowSchemeAsync(
    schemeId: "10001",
    name: "Updated Name",
    description: "Updated description");

// Delete a workflow scheme
await jiraClient.WorkflowSchemes.DeleteWorkflowSchemeAsync("10001");
```

### Project Status Queries

```csharp
// Get all statuses for a project (grouped by issue type)
var projectStatuses = await jiraClient.ProjectStatuses.GetProjectStatusesAsync("PROJ");

foreach (var issueTypeStatuses in projectStatuses)
{
    Console.WriteLine($"Issue Type: {issueTypeStatuses.Name}");
    foreach (var status in issueTypeStatuses.Statuses)
    {
        Console.WriteLine($"  - {status.Name}");
    }
}
```

## Download

- [Get the latest via NuGet](http://nuget.org/List/Packages/Jira.Api).

## License

This project is licensed under [BSD](/LICENSE.md).

## Dependencies & Requirements

- [RestSharp](https://www.nuget.org/packages/RestSharp)
- [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json)
- Tested with JIRA v9.12.2
	- We can't move to Jira Cloud, so we use our now-unsupported server version.
	- Happy to receive a complementary 25-user Datacenter license for our troubles, Atlassian?

## History

- 2025 - we needed Personal Access Tokens (PATs) support, so we forked and modernized the project.  As we progressed, we found many other improvements that we wanted to make, so we made them, even at the expense of breaking changes.

- For a description changes, check out the [Change History Page](/docs/change-history.md).

- Federico's project began in 2010 during a [ShipIt](https://www.atlassian.com/company/shipit) day at Atlassian with provider
  to query Jira issues using LINQ syntax. Over time it grew to add many more operations on top of the JIRA SOAP API.
  Support of REST API was added on v4.0 and support of SOAP API was dropped on v8.0.

## Related Projects

- [Atlassian.NET.SDK](https://bitbucket.org/farmas/atlassian.net-sdk/) - [Federico Silva Armas](https://bitbucket.org/farmas/workspace/repositories/)'s project that we forked from (no longer maintained)

