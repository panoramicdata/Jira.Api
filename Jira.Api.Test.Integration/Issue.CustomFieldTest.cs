using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Jira.Api.Test.Integration;

public class IssueCustomFieldTest
{
	private readonly Random _random = new();

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CustomFieldsForProject_IfProjectDoesNotExist_ShouldThrowException(Jira jira)
	{
		var options = new CustomFieldFetchOptions();
		options.ProjectKeys.Add("FOO");
		Exception ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await jira.Fields.GetCustomFieldsAsync(options, default));

		Assert.Contains("Project with key 'FOO' was not found on the Jira server.", ex.Message);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CustomFieldsForProject_ShouldReturnAllCustomFieldsOfAllIssueTypes(Jira jira)
	{
		var options = new CustomFieldFetchOptions();
		options.ProjectKeys.Add("TST");
		var results = await jira.Fields.GetCustomFieldsAsync(options, default);
		Assert.Equal(21, results.Count());
	}

	/// <summary>
	/// Note that in the current data set all the custom fields are reused between the issue types.
	/// </summary>
	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CustomFieldsForProjectAndIssueType_ShouldReturnAllCustomFieldsTheIssueType(Jira jira)
	{
		var options = new CustomFieldFetchOptions();
		options.ProjectKeys.Add("TST");
		options.IssueTypeNames.Add("Bug");

		var results = await jira.Fields.GetCustomFieldsAsync(options, default);
		Assert.Equal(19, results.Count());
	}

	/// <summary>
	/// This case test the path when there are multiple custom fields defined in JIRA with the same name.
	/// Most likely because the user has a combination of Classic and NextGen projects. Since the test
	/// integration server is unable to create these type of custom fields, a property was added to the
	/// CustomFieldValueCollection that can force the new code path to execute.
	/// </summary>
	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CanSetCustomFieldUsingSearchByProjectOnly(Jira jira)
	{
		var summaryValue = "Test issue with custom field by project" + _random.Next(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		issue.CustomFields.SearchByProjectOnly = true;
		issue["Custom Text Field"] = "My new value";
		issue["Custom Date Field"] = "2015-10-03";

		var newIssue = await issue.SaveChangesAsync(default);

		Assert.Equal("My new value", newIssue["Custom Text Field"]);
		Assert.Equal("2015-10-03", newIssue["Custom Date Field"]);
	}

	[Fact]
	public async Task CanHandleCustomFieldWithoutSerializerThatIsArrayOfObjects()
	{
		var jira = Jira.CreateRestClient(new TraceReplayer("Trace_CustomFieldArrayOfObjects.txt"));
		var issue = (await jira.Issues.GetIssuesFromJqlAsync("foo", 0, null, default)).Single();

		Assert.True(issue["Watchers"].Value.Length > 0);
	}

	[Fact]
	public async Task CanHandleCustomFieldSetToEmptyArrayByDefaultFromServer()
	{
		// See: https://bitbucket.org/farmas/atlassian.net-sdk/issues/372
		var jira = Jira.CreateRestClient(new TraceReplayer("Trace_CustomFieldEmptyArray.txt"));
		var issue = await jira.Issues.GetIssueAsync("GIT-103", default);

		issue.Summary = "Some change";
		await issue.SaveChangesAsync(default);

		Assert.NotNull(issue);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task AddAndReadCustomFieldById(Jira jira)
	{
		var summaryValue = "Test issue with custom text" + _random.Next(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		issue.CustomFields.AddById("customfield_10000", "My Sample Text");
		await issue.SaveChangesAsync(default);

		var newIssue = await jira.Issues.GetIssueAsync(issue.Key.Value, default);
		Assert.Equal("My Sample Text", newIssue.CustomFields.First(f => f.Id.Equals("customfield_10000")).Values.First());
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateIssueWithCascadingSelectFieldWithOnlyParentOptionSet(Jira jira)
	{
		var summaryValue = "Test issue with cascading select" + _random.Next(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		// Add cascading select with only parent set.
		await issue.CustomFields.AddCascadingSelectFieldAsync("Custom Cascading Select Field", "Option3", null, default);
		await issue.SaveChangesAsync(default);

		var newIssue = await jira.Issues.GetIssueAsync(issue.Key.Value, default);

		var cascadingSelect = newIssue.CustomFields.GetCascadingSelectField("Custom Cascading Select Field");
		Assert.Equal("Option3", cascadingSelect.ParentOption);
		Assert.Null(cascadingSelect.ChildOption);
		Assert.Equal("Custom Cascading Select Field", cascadingSelect.Name);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateAndQueryIssueWithLargeNumberCustomField(Jira jira)
	{
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = "Test issue with large custom field number" + _random.Next(int.MaxValue),
			Assignee = "admin"
		};

		issue["Custom Number Field"] = "10000000000";
		await issue.SaveChangesAsync(default);

		var newIssue = await jira.Issues.GetIssueAsync(issue.Key.Value, default);
		Assert.Equal("10000000000", newIssue["Custom Number Field"]);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateAndQueryIssueWithComplexCustomFields(Jira jira)
	{
		var dateTime = new DateTime(2016, 11, 11, 11, 11, 0);
		var dateTimeStr = dateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffzzz");
		dateTimeStr = dateTimeStr.Remove(dateTimeStr.LastIndexOf(":"), 1);

		var summaryValue = "Test issue with lots of custom fields (Created)" + _random.Next(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		issue["Custom Text Field"] = "My new value";
		issue["Custom Date Field"] = "2015-10-03";
		issue["Custom DateTime Field"] = dateTimeStr;
		issue["Custom User Field"] = "admin";
		issue["Custom Select Field"] = "Blue";
		issue["Custom Group Field"] = "jira-users";
		issue["Custom Project Field"] = "TST";
		issue["Custom Version Field"] = "1.0";
		issue["Custom Radio Field"] = "option1";
		issue["Custom Number Field"] = "12.34";
		await issue.CustomFields.AddArrayAsync("Custom Labels Field", ["label1", "label2"], default);
		await issue.CustomFields.AddArrayAsync("Custom Multi Group Field", ["jira-developers", "jira-users"], default);
		await issue.CustomFields.AddArrayAsync("Custom Multi Select Field", ["option1", "option2"], default);
		await issue.CustomFields.AddArrayAsync("Custom Multi User Field", ["admin", "test"], default);
		await issue.CustomFields.AddArrayAsync("Custom Checkboxes Field", ["option1", "option2"], default);
		await issue.CustomFields.AddArrayAsync("Custom Multi Version Field", ["2.0", "3.0"], default);
		await issue.CustomFields.AddCascadingSelectFieldAsync("Custom Cascading Select Field", "Option2", "Option2.2", default);

		await issue.SaveChangesAsync(default);

		var newIssue = await jira.Issues.GetIssueAsync(issue.Key.Value, default);

		Assert.Equal("My new value", newIssue["Custom Text Field"]);
		Assert.Equal("2015-10-03", newIssue["Custom Date Field"]);
		Assert.Equal("admin", newIssue["Custom User Field"]);
		Assert.Equal("Blue", newIssue["Custom Select Field"]);
		Assert.Equal("jira-users", newIssue["Custom Group Field"]);
		Assert.Equal("TST", newIssue["Custom Project Field"]);
		Assert.Equal("1.0", newIssue["Custom Version Field"]);
		Assert.Equal("option1", newIssue["Custom Radio Field"]);
		Assert.Equal("12.34", newIssue["Custom Number Field"]);
		Assert.Equal("admin@example.com", newIssue.CustomFields.GetAs<JiraUser>("Custom User Field").Email);

		var serverDate = DateTime.Parse(newIssue["Custom DateTime Field"].Value);
		Assert.Equal(dateTime, serverDate);

		Assert.Equal(new string[2] { "label1", "label2" }, newIssue.CustomFields["Custom Labels Field"].Values);
		Assert.Equal(new string[2] { "jira-developers", "jira-users" }, newIssue.CustomFields["Custom Multi Group Field"].Values);
		Assert.Equal(new string[2] { "option1", "option2" }, newIssue.CustomFields["Custom Multi Select Field"].Values);
		Assert.Equal(new string[2] { "admin", "test" }, newIssue.CustomFields["Custom Multi User Field"].Values);
		Assert.Equal(new string[2] { "option1", "option2" }, newIssue.CustomFields["Custom Checkboxes Field"].Values);
		Assert.Equal(new string[2] { "2.0", "3.0" }, newIssue.CustomFields["Custom Multi Version Field"].Values);

		var users = newIssue.CustomFields.GetAs<JiraUser[]>("Custom Multi User Field");
		Assert.Contains(users, u => u.Email == "test@qa.com");

		var cascadingSelect = newIssue.CustomFields.GetCascadingSelectField("Custom Cascading Select Field");
		Assert.Equal("Option2", cascadingSelect.ParentOption);
		Assert.Equal("Option2.2", cascadingSelect.ChildOption);
		Assert.Equal("Custom Cascading Select Field", cascadingSelect.Name);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CanClearValueOfCustomField(Jira jira)
	{
		var summaryValue = "Test issue " + _random.Next(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		issue["Custom Text Field"] = "My new value";
		issue["Custom Date Field"] = "2015-10-03";
		issue["Custom Select Field"] = "Blue";
		await issue.SaveChangesAsync(default);

		var newIssue = await jira.Issues.GetIssueAsync(issue.Key.Value, default);
		Assert.Equal("My new value", newIssue["Custom Text Field"]);
		Assert.Equal("2015-10-03", newIssue["Custom Date Field"]);
		newIssue["Custom Text Field"] = null;
		newIssue["Custom Date Field"] = null;
		newIssue["Custom Select Field"] = null;
		await newIssue.SaveChangesAsync(default);

		var updatedIssue = await jira.Issues.GetIssueAsync(issue.Key.Value, default);

		Assert.Null(updatedIssue["Custom Text Field"]);
		Assert.Null(updatedIssue["Custom Date Field"]);
		Assert.Null(updatedIssue["Custom Select Field"]);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateAndUpdateIssueWithComplexCustomFields(Jira jira)
	{
		var dateTime = new DateTime(2016, 11, 11, 11, 11, 0);
		var dateTimeStr = dateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffzzz");
		dateTimeStr = dateTimeStr.Remove(dateTimeStr.LastIndexOf(":"), 1);
		var summaryValue = "Test issue with lots of custom fields (Created)" + _random.Next(int.MaxValue);

		// Create issue with no custom fields set
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		await issue.SaveChangesAsync(default);

		// Retrieve the issue, set all custom fields and save the changes.
		var newIssue = await jira.Issues.GetIssueAsync(issue.Key.Value, default);

		newIssue["Custom Text Field"] = "My new value";
		newIssue["Custom Date Field"] = "2015-10-03";
		newIssue["Custom DateTime Field"] = dateTimeStr;
		newIssue["Custom User Field"] = "admin";
		newIssue["Custom Select Field"] = "Blue";
		newIssue["Custom Group Field"] = "jira-users";
		newIssue["Custom Project Field"] = "TST";
		newIssue["Custom Version Field"] = "1.0";
		newIssue["Custom Radio Field"] = "option1";
		newIssue["Custom Number Field"] = "1234";
		await newIssue.CustomFields.AddArrayAsync("Custom Labels Field", ["label1", "label2"], default);
		await newIssue.CustomFields.AddArrayAsync("Custom Multi Group Field", ["jira-developers", "jira-users"], default);
		await newIssue.CustomFields.AddArrayAsync("Custom Multi Select Field", ["option1", "option2"], default);
		await newIssue.CustomFields.AddArrayAsync("Custom Multi User Field", ["admin", "test"], default);
		await newIssue.CustomFields.AddArrayAsync("Custom Checkboxes Field", ["option1", "option2"], default);
		await newIssue.CustomFields.AddArrayAsync("Custom Multi Version Field", ["2.0", "3.0"], default);
		await newIssue.CustomFields.AddCascadingSelectFieldAsync("Custom Cascading Select Field", "Option2", "Option2.2", default);

		await newIssue.SaveChangesAsync(default);

		// Retrieve the issue again and verify fields
		var updatedIssue = await jira.Issues.GetIssueAsync(issue.Key.Value, default);

		Assert.Equal("My new value", updatedIssue["Custom Text Field"]);
		Assert.Equal("2015-10-03", updatedIssue["Custom Date Field"]);
		Assert.Equal("admin", updatedIssue["Custom User Field"]);
		Assert.Equal("Blue", updatedIssue["Custom Select Field"]);
		Assert.Equal("jira-users", updatedIssue["Custom Group Field"]);
		Assert.Equal("TST", updatedIssue["Custom Project Field"]);
		Assert.Equal("1.0", updatedIssue["Custom Version Field"]);
		Assert.Equal("option1", updatedIssue["Custom Radio Field"]);
		Assert.Equal("1234", updatedIssue["Custom Number Field"]);

		var serverDate = DateTime.Parse(updatedIssue["Custom DateTime Field"].Value);
		Assert.Equal(dateTime, serverDate);

		Assert.Equal(new string[2] { "label1", "label2" }, updatedIssue.CustomFields["Custom Labels Field"].Values);
		Assert.Equal(new string[2] { "jira-developers", "jira-users" }, updatedIssue.CustomFields["Custom Multi Group Field"].Values);
		Assert.Equal(new string[2] { "option1", "option2" }, updatedIssue.CustomFields["Custom Multi Select Field"].Values);
		Assert.Equal(new string[2] { "admin", "test" }, updatedIssue.CustomFields["Custom Multi User Field"].Values);
		Assert.Equal(new string[2] { "option1", "option2" }, updatedIssue.CustomFields["Custom Checkboxes Field"].Values);
		Assert.Equal(new string[2] { "2.0", "3.0" }, updatedIssue.CustomFields["Custom Multi Version Field"].Values);

		var cascadingSelect = updatedIssue.CustomFields.GetCascadingSelectField("Custom Cascading Select Field");
		Assert.Equal("Option2", cascadingSelect.ParentOption);
		Assert.Equal("Option2.2", cascadingSelect.ChildOption);
		Assert.Equal("Custom Cascading Select Field", cascadingSelect.Name);

		// Update custom fields again and save
		updatedIssue["Custom Text Field"] = "My newest value";
		updatedIssue["Custom Date Field"] = "2019-10-03";
		updatedIssue["Custom Number Field"] = "9999";
		updatedIssue.CustomFields["Custom Labels Field"].Values = ["label3"];
		await updatedIssue.SaveChangesAsync(default);

		// Retrieve the issue one last time and verify custom fields.
		var updatedIssue2 = await jira.Issues.GetIssueAsync(issue.Key.Value, default);
		Assert.Equal("My newest value", updatedIssue["Custom Text Field"]);
		Assert.Equal("2019-10-03", updatedIssue["Custom Date Field"]);
		Assert.Equal("9999", updatedIssue2["Custom Number Field"]);
		Assert.Equal(new string[1] { "label3" }, updatedIssue.CustomFields["Custom Labels Field"].Values);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CreateAndQuerySprintName(Jira jira)
	{
		var issue = new Issue(jira, "SCRUM")
		{
			Type = "Bug",
			Summary = "Test issue with sprint" + _random.Next(int.MaxValue),
			Assignee = "admin"
		};
		// Set the sprint by id
		issue["Sprint"] = "1";
		await issue.SaveChangesAsync(default);

		// Get the sprint by name
		var newIssue = await jira.Issues.GetIssueAsync(issue.Key.Value, default);
		Assert.Equal("Sprint 1", newIssue["Sprint"]);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task UpdateAndQuerySprintName(Jira jira)
	{
		var issue = new Issue(jira, "SCRUM")
		{
			Type = "Bug",
			Summary = "Test issue with sprint" + _random.Next(int.MaxValue),
			Assignee = "admin"
		};
		await issue.SaveChangesAsync(default);
		Assert.Null(issue["Sprint"]);

		// Set the sprint by id
		issue["Sprint"] = "1";
		await issue.SaveChangesAsync(default);

		// Get the sprint by name
		var newIssue = await jira.Issues.GetIssueAsync(issue.Key.Value, default);
		Assert.Equal("Sprint 1", newIssue["Sprint"]);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CanUpdateIssueWithoutModifyingCustomFields(Jira jira)
	{
		var issue = new Issue(jira, "SCRUM")
		{
			Type = "Bug",
			Summary = "Test issue with sprint" + _random.Next(int.MaxValue),
			Assignee = "admin"
		};
		issue["Sprint"] = "1";
		await issue.SaveChangesAsync(default);
		Assert.Equal("Sprint 1", issue["Sprint"]);

		issue.Summary += " (Updated)";
		await issue.SaveChangesAsync(default);
		Assert.Equal("Sprint 1", issue["Sprint"]);
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task ThrowsErrorWhenSettingSprintByName(Jira jira)
	{
		var issue = new Issue(jira, "SCRUM")
		{
			Type = "Bug",
			Summary = "Test issue with sprint" + _random.Next(int.MaxValue),
			Assignee = "admin"
		};

		// Set the sprint by name
		issue["Sprint"] = "Sprint 1";

		try
		{
			await issue.SaveChangesAsync(default);
			throw new Exception("Method did not throw exception");
		}
		catch (AggregateException ex)
		{
			Assert.Contains("Number value expected as the Sprint id", ex.Flatten().InnerException.Message);
		}
	}

	public class IssueFieldMetadataCustomFieldOption
	{
		[JsonProperty("id")]
		public long Id { get; set; }

		[JsonProperty("value")]
		public string Value { get; set; }

		[JsonProperty("self")]
		public string Self { get; set; }
	}
}
