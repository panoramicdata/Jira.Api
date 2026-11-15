using Jira.Api.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;

namespace Jira.Api.Models;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2360:Optional parameters should not be used", Justification = "Published public API: replacing optional parameters with overloads is a breaking change for existing consumers.")]
public partial class Issue
{
	private async Task PopulateNamedEntitiesAsync(RemoteIssue remote, CancellationToken cancellationToken)
	{
		if (Status != null)
		{
			await Status.LoadIdAndNameAsync(Jira, cancellationToken).ConfigureAwait(false);
			remote.status = new RemoteStatus { id = Status.Id, name = Status.Name };
		}

		if (Resolution != null)
		{
			await Resolution.LoadIdAndNameAsync(Jira, cancellationToken).ConfigureAwait(false);
			remote.resolution = new RemoteResolution { id = Resolution.Id, name = Resolution.Name };
		}

		if (Priority != null)
		{
			await Priority.LoadIdAndNameAsync(Jira, cancellationToken).ConfigureAwait(false);
			remote.priority = new RemotePriority { id = Priority.Id, name = Priority.Name };
		}

		if (Type != null)
		{
			await Type.LoadIdAndNameAsync(Jira, cancellationToken).ConfigureAwait(false);
			remote.type = new RemoteIssueType { id = Type.Id, name = Type.Name };
		}
	}

	private void PopulateCollections(RemoteIssue remote)
	{
		if (AffectsVersions.Count > 0)
		{
			remote.affectsVersions = [.. AffectsVersions.Select(v => v.RemoteVersion)];
		}

		if (FixVersions.Count > 0)
		{
			remote.fixVersions = [.. FixVersions.Select(v => v.RemoteVersion)];
		}

		if (Components.Count > 0)
		{
			remote.components = [.. Components.Select(c => c.RemoteComponent)];
		}

		if (CustomFields.Count > 0)
		{
			remote.customFieldValues = [.. CustomFields.Select(f => new RemoteCustomFieldValue
			{
				customfieldId = f.Id,
				values = f.Values,
				serializer = f.Serializer
			})];
		}

		if (Labels.Count > 0)
		{
			remote.labels = [.. Labels];
		}
	}

	private async Task<string> GetStringValueForPropertyAsync(object container, PropertyInfo property, CancellationToken cancellationToken)
	{
		var value = property.GetValue(container, null);

		if (property.PropertyType == typeof(DateTime?))
		{
			return FormatShortDate((DateTime?)value);
		}

		if (typeof(JiraNamedEntity).IsAssignableFrom(property.PropertyType))
		{
			return await GetJiraNamedEntityIdAsync(container, property, cancellationToken).ConfigureAwait(false);
		}

		if (typeof(AbstractNamedRemoteEntity).IsAssignableFrom(property.PropertyType))
		{
			return GetRemoteEntityId(value);
		}

		return value?.ToString();
	}

	private static string FormatShortDate(DateTime? dateValue) => dateValue.HasValue ? dateValue.Value.ToString("d/MMM/yy") : null;

	private static string GetRemoteEntityId(object value) => (value as AbstractNamedRemoteEntity)?.id;

	private async Task<string> GetJiraNamedEntityIdAsync(object container, PropertyInfo property, CancellationToken cancellationToken)
	{
		if (property.GetValue(container, null) is JiraNamedEntity jiraNamedEntity)
		{
			await jiraNamedEntity.LoadIdAndNameAsync(Jira, cancellationToken).ConfigureAwait(false);
			return jiraNamedEntity.Id;
		}

		return null;
	}
}
