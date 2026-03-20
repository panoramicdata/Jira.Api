# How to configure the User-Agent

## Overview

The `JiraRestClientSettings` class requires a **User-Agent** string that is sent with every HTTP request to your Jira server. This helps Jira administrators identify which applications are making API calls.

## Quick Start

```csharp
var settings = new JiraRestClientSettings("MyApp/1.0");
var jira = JiraClient.CreateRestClient("http://your-jira-server", "user", "password", settings);
```

## User-Agent Format

The User-Agent value must conform to the [RFC 9110](https://www.rfc-editor.org/rfc/rfc9110#name-user-agent) product token format:

- **Simple name**: `MyApp`
- **Name with version**: `MyApp/1.0`
- **Multiple products**: `MyApp/1.0 RestLib/2.3`

### Valid characters

Product names and versions may contain: letters, digits, `.`, `-`, `_`, `~`

Product names must start with a letter or digit.

### Examples

| Value | Valid? |
|---|---|
| `MyApp` | ✅ |
| `MyApp/1.0` | ✅ |
| `MyApp/1.0-beta` | ✅ |
| `My_App/1.0` | ✅ |
| `MyApp/1.0 OtherLib/2.0` | ✅ |
| `.MyApp` | ❌ Starts with `.` |
| `My@App` | ❌ Contains `@` |
| `MyApp/` | ❌ Missing version after `/` |
| `/1.0` | ❌ Missing product name |

## Build Warning

If you use the parameterless `JiraRestClientSettings()` constructor, you will receive a **CS0618 build warning** prompting you to provide a User-Agent:

```
warning CS0618: 'JiraRestClientSettings.JiraRestClientSettings()' is obsolete:
'Use the constructor that accepts a userAgent parameter to identify your application
(e.g. new JiraRestClientSettings("MyApp/1.0")).'
```

The parameterless constructor still works and uses the default User-Agent `"JiraApi_Application"`, but providing your own is recommended.

## Validation Errors

If you provide an invalid User-Agent format, a `FormatException` is thrown at construction time:

```csharp
// Throws FormatException
var settings = new JiraRestClientSettings(".InvalidApp");
```

If you provide `null` or whitespace, an `ArgumentException` is thrown:

```csharp
// Throws ArgumentNullException
var settings = new JiraRestClientSettings(null!);

// Throws ArgumentException
var settings = new JiraRestClientSettings("");
```

## Using with OAuth

```csharp
var settings = new JiraRestClientSettings("MyApp/1.0");
var jira = JiraClient.CreateOAuthRestClient(
    url,
    consumerKey,
    consumerSecret,
    accessToken,
    tokenSecret,
    settings: settings);
```

## Using with Personal Access Tokens

```csharp
var settings = new JiraRestClientSettings("MyApp/1.0");
var jira = JiraClient.CreateRestClient("http://your-jira-server", personalAccessToken, settings);
```
