using Jira.Api.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;

namespace Jira.Api.Models;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2360:Optional parameters should not be used", Justification = "Published public API: replacing optional parameters with overloads is a breaking change for existing consumers.")]
public partial class Issue
{
	/// <summary>
	/// Gets the RemoteFields representing the fields that were updated
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for this operation.</param>
	async Task<RemoteFieldValue[]> IRemoteIssueFieldProvider.GetRemoteFieldValuesAsync(CancellationToken cancellationToken)
	{
		var fields = new List<RemoteFieldValue>();

		var remoteFields = typeof(RemoteIssue).GetProperties();
		foreach (var localProperty in typeof(Issue).GetProperties())
		{
			if (typeof(IRemoteIssueFieldProvider).IsAssignableFrom(localProperty.PropertyType))
			{
				if (localProperty.GetValue(this, null) is IRemoteIssueFieldProvider fieldsProvider)
				{
					var remoteFieldValues = await fieldsProvider.GetRemoteFieldValuesAsync(cancellationToken).ConfigureAwait(false);
					fields.AddRange(remoteFieldValues);
				}
			}
			else
			{
				var remoteProperty = remoteFields.FirstOrDefault(i => i.Name.Equals(localProperty.Name, StringComparison.OrdinalIgnoreCase));
				if (remoteProperty == null)
				{
					continue;
				}

				var localStringValue = await GetStringValueForPropertyAsync(this, localProperty, cancellationToken).ConfigureAwait(false);
				var remoteStringValue = await GetStringValueForPropertyAsync(OriginalRemoteIssue, remoteProperty, cancellationToken).ConfigureAwait(false);

				if (remoteStringValue != localStringValue)
				{
					var remoteFieldName = remoteProperty.Name;

					var remoteFieldNameAttr = localProperty.GetCustomAttributes(typeof(RemoteFieldNameAttribute), true).OfType<RemoteFieldNameAttribute>().FirstOrDefault();
					if (remoteFieldNameAttr != null)
					{
						remoteFieldName = remoteFieldNameAttr.Name;
					}

					fields.Add(new RemoteFieldValue
					{
						id = remoteFieldName,
						values = [localStringValue]
					});
				}
			}
		}

		return [.. fields];
	}

	internal async Task<RemoteIssue> ToRemoteIssueAsync(CancellationToken cancellationToken)
	{
		var remote = new RemoteIssue
		{
			assignee = Assignee,
			description = Description,
			environment = Environment,
			project = Project,
			reporter = Reporter,
			summary = Summary,
			votesData = Votes != null ? new RemoteVotes { hasVoted = HasUserVoted == true, votes = Votes.Value } : null,
			duedate = DueDate,
			timeTracking = TimeTrackingData,
			key = Key?.Value
		};

		await PopulateNamedEntitiesAsync(remote, cancellationToken).ConfigureAwait(false);
		PopulateCollections(remote);

		return remote;
	}
}
