# Atlassian.NET SDK

Contains utilities for interacting with  [Atlassian JIRA](http://www.atlassian.com/software/jira).

## Support Notice

All features tested on JIRA v9.12.2

## NEW IN Jira.Api...

If you are migrating from Atlassian.SDK, there are MANY breaking changes, we believe all for the better.
The main items:

* The namespace has changed from `Atlassian.SDK` to `Jira.Api`.
* EVERYTHING is asynchronous now, no more sync methods hiding async under the covers.
* You MUST provide cancellation tokens to all async methods.
* There are no more optional parameters, all parameters are required.
* The order of skip and take parameters has been changed for everyone's sanity.
	- The new order is `skip, take` instead of `take, skip`.
	- This is a nasty one if you are already specifying these value.
	- Sorry, but that's the way it is!
* Attachment uploads use FileInfo instead of full path file names.

## Download

- [Get the latest via NuGet](http://nuget.org/List/Packages/Atlassian.SDK).
- [Get the latest binaries from AppVeyor](https://ci.appveyor.com/project/farmas/atlassian-net-sdk/history).
  [![Build Status](https://ci.appveyor.com/api/projects/status/bitbucket/farmas/atlassian.net-sdk?branch=release&amp;svg=true)](https://ci.appveyor.com/project/farmas/atlassian-net-sdk)

## License

This project is licensed under  [BSD](/LICENSE.md).

## Dependencies & Requirements

- [RestSharp](https://www.nuget.org/packages/RestSharp)
- [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json)
- Tested with JIRA v9.12.2

## History

- For a description changes, check out the [Change History Page](/docs/change-history.md).

- This project began in 2010 during a [ShipIt](https://www.atlassian.com/company/shipit) day at Atlassian with provider
  to query Jira issues using LINQ syntax. Over time it grew to add many more operations on top of the JIRA SOAP API.
  Support of REST API was added on v4.0 and support of SOAP API was dropped on v8.0.

## Related Projects

- [VS Jira](https://bitbucket.org/farmas/vsjira) - A VisualStudio Extension that adds tools to interact with JIRA
servers.
- [Jira OAuth CLI](https://bitbucket.org/farmas/atlassian.net-jira-oauth-cli) - Command line tool to setup OAuth on a JIRA server so that it can be used with the Atlassian.NET SDK.

## Signed Version

### Atlassian.SDK.Signed (Deprecated)

The [Atlassian.SDK.Signed](https://www.nuget.org/packages/Atlassian.SDK.Signed/) package contains a signed version of
the assembly, however it is no longer being mantained. It has the following limitations:

- It references the  [RestSharpSigned](https://www.nuget.org/packages/RestSharpSigned) package, which is not up-to-date
  to the official  [RestSharp](https://www.nuget.org/packages/RestSharpSigned) package.
- It only supports net452 framework (does not support .netcore).

### Using StrongNameSigner

An alternative to using the Atlassian.SDK.Signed package is to use the [StrongNameSigner](https://www.nuget.org/packages/Brutal.Dev.StrongNameSigner) which can automatically sign any un-signed packages in your project. For a sample of how to use it in a project see [VS Jira](https://bitbucket.org/farmas/vsjira).

## Documentation

The documentation is placed under the [docs](/docs) folder.

As a first user, here is the documentation on [how to use the SDK](/docs/how-to-use-the-sdk.md).
