# Change History

## Version 9.3.2 (09/07/2026)

- **Dashboards service**: Added `IDashboardService` (available as `JiraClient.Dashboards`) with:
  - `GetDashboardsAsync(skip, take)` — page through dashboards visible to the current user via `GET /rest/api/2/dashboard`.
  - `GetDashboardAsync(dashboardId)` — get a dashboard's id, name and view URL via `GET /rest/api/2/dashboard/{id}`.
  - `GetDashboardDetailAsync(dashboardId)` — get a dashboard's gadgets and their user preferences via `GET /rest/dashboards/1.0/{id}`. Note: this is a JIRA Server internal endpoint (the one the JIRA web UI itself uses); it is not part of the public REST API and may not be available on JIRA Cloud.
  - `JiraDashboardGadget` exposes convenience accessors for Filter Results gadgets: `IsFilterResults`, `FilterId`, `ColumnNames`, `NumberToShow` and `RefreshMinutes`, all parsed tolerantly from the gadget's user preferences.
- Covered by unit tests and read-only integration tests (`--filter "Category=ReadOnly"`), verified against JIRA Server 9.12.2.

## Version 9.2.25 (22/05/2026)

- **Get attachment by ID**: Added `IIssueService.GetAttachmentAsync(attachmentId)` to retrieve a single attachment's metadata directly by ID via `GET /rest/api/2/attachment/{id}`, without needing to fetch all attachments for an issue first. Resolves [Issue #4](https://github.com/panoramicdata/Jira.Api/issues/4).

## Version 9.2.24 (10/05/2026)

- **Issue reranking API**: Added `IIssueService.ReRankAsync(...)` to support Jira Agile rank operations via `PUT /rest/agile/1.0/issue/rank`.
  - Supports moving one or more issues relative to `rankBeforeIssue` or `rankAfterIssue`.
  - Includes parameter validation to ensure exactly one anchor is provided.
  - Covered by integration testing against Jira 9.12.2.

## Version 9.2.21 (04/05/2026)

- **Indexing service**: Implemented indexing service and related models.

## Version 9.2.19 (17/04/2026)

- CI: removed test step from pipeline.

## Version 9.2.18 (16/04/2026)

- NuGet governance remediation.

## Version 9.2.16 (05/04/2026)

- CI/CD configuration updates and solution structure improvements.

## Version 9.2.14 (03/04/2026)

- **User-Agent support**: `JiraRestClientSettings` now requires a `userAgent` string parameter. The parameterless constructor is marked `[Obsolete]` and emits build warning CS0618. The value is validated against RFC 9110 product token format and sent with every HTTP request.
- Standardized tests on AwesomeAssertions.
- Updated to .NET SDK 10.0.102.
- Governance remediation: added CI/CD, ImplicitUsings, and community files.