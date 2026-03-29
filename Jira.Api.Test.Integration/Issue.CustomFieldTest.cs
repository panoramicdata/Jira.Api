namespace Jira.Api.Test.Integration;

[Trait("Category", "WritesToApi")]
public class IssueCustomFieldTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CustomFieldsForProject_IfProjectDoesNotExist_ShouldThrowException(JiraClient jira)
	{
		var options = new CustomFieldFetchOptions();
		options.ProjectKeys.Add("FOO");
		var act = async () => await jira.Fields.GetCustomFieldsAsync(options, CancellationToken);
		var ex = await act.Should().ThrowExactlyAsync<InvalidOperationException>();

		ex.Which.Message.Should().Contain("Project with key 'FOO' was not found on the JiraClient server.");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CustomFieldsForProject_ShouldReturnAllCustomFieldsOfAllIssueTypes(JiraClient jira)
	{
		var options = new CustomFieldFetchOptions();
		options.ProjectKeys.Add("TST");
		var results = await jira.Fields.GetCustomFieldsAsync(options, CancellationToken);
		results.Should().HaveCount(21);
	}

	/// <summary>
	/// Note that in the current data set all the custom fields are reused between the issue types.
	/// </summary>
	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task CustomFieldsForProjectAndIssueType_ShouldReturnAllCustomFieldsTheIssueType(JiraClient jira)
	{
		var options = new CustomFieldFetchOptions();
		options.ProjectKeys.Add("TST");
		options.IssueTypeNames.Add("Bug");

		var results = await jira.Fields.GetCustomFieldsAsync(options, CancellationToken);
		results.Should().HaveCount(19);
	}

	/// <summary>
	/// This case test the path when there are multiple custom fields defined in JIRA with the same name.
	/// Most likely because the user has a combination of Classic and NextGen projects. Since the test
	/// integration server is unable to create these type of custom fields, a property was added to the
	/// CustomFieldValueCollection that can force the new code path to execute.
	/// </summary>
	[Theory]
	[ClassData(typeof(JiraProvider))]
   [Trait("Category", "WritesToApi")]
	public async Task CanSetCustomFieldUsingSearchByProjectOnly(JiraClient jira)
	{
		var summaryValue = "Test issue with custom field by project" + RandomNumberGenerator.GetInt32(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		issue.CustomFields.SearchByProjectOnly = true;
		issue["Custom Text Field"] = "My new value";
		issue["Custom Date Field"] = "2015-10-03";

		var newIssue = await issue.SaveChangesAsync(CancellationToken);

		newIssue["Custom Text Field"].Should().Be("My new value");
		newIssue["Custom Date Field"].Should().Be("2015-10-03");
		}

	[Fact]
	public async Task CanHandleCustomFieldWithoutSerializerThatIsArrayOfObjects()
	{
		var jira = JiraClient.CreateRestClient(new TraceReplayer("Trace_CustomFieldArrayOfObjects.txt"));
		var issue = (await jira.Issues.GetIssuesFromJqlAsync("foo", 0, null, CancellationToken)).Single();
		var watchers = issue["Watchers"];
		watchers.Should().NotBeNull();
		(watchers!.Value.Length > 0).Should().BeTrue();
	}

	[Fact]
	public async Task CanHandleCustomFieldSetToEmptyArrayByCancellationTokenFromServer()
	{
		// See: https://bitbucket.org/farmas/atlassian.net-sdk/issues/372
		var jira = JiraClient.CreateRestClient(new TraceReplayer("Trace_CustomFieldEmptyArray.txt"));
		var issue = await jira.Issues.GetIssueAsync("GIT-103", CancellationToken);

		issue.Summary = "Some change";
		await issue.SaveChangesAsync(CancellationToken);

		issue.Should().NotBeNull();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
   [Trait("Category", "WritesToApi")]
	public async Task AddAndReadCustomFieldById(JiraClient jira)
	{
		var summaryValue = "Test issue with custom text" + RandomNumberGenerator.GetInt32(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		issue.CustomFields.AddById("customfield_10000", ["My Sample Text"]);
		await issue.SaveChangesAsync(CancellationToken);

		var newIssue = await jira.Issues.GetIssueAsync(issue.Key.Value, CancellationToken);
		newIssue.CustomFields.First(f => f.Id.Equals("customfield_10000")).Values.First().Should().Be("My Sample Text");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
    [Trait("Category", "WritesToApi")]
	public async Task CreateIssueWithCascadingSelectFieldWithOnlyParentOptionSet(JiraClient jira)
	{
		var summaryValue = "Test issue with cascading select" + RandomNumberGenerator.GetInt32(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		// Add cascading select with only parent set.
		issue.CustomFields.AddCascadingSelectField("Custom Cascading Select Field", "Option3", null);
		await issue.SaveChangesAsync(CancellationToken);

		var newIssue = await jira.Issues.GetIssueAsync(issue.Key.Value, CancellationToken);

		var cascadingSelect = newIssue.CustomFields.GetCascadingSelectField("Custom Cascading Select Field");
		cascadingSelect.Should().NotBeNull();
		cascadingSelect.ParentOption.Should().Be("Option3");
		cascadingSelect.ChildOption.Should().BeNull();
		cascadingSelect.Name.Should().Be("Custom Cascading Select Field");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
   [Trait("Category", "WritesToApi")]
	public async Task CreateAndQueryIssueWithLargeNumberCustomField(JiraClient jira)
	{
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = "Test issue with large custom field number" + RandomNumberGenerator.GetInt32(int.MaxValue),
			Assignee = "admin"
		};

		issue["Custom Number Field"] = "10000000000";
		await issue.SaveChangesAsync(CancellationToken);

		var newIssue = await jira.Issues.GetIssueAsync(issue.Key.Value, CancellationToken);
		newIssue["Custom Number Field"].Should().Be("10000000000");
		}

	[Theory]
	[ClassData(typeof(JiraProvider))]
    [Trait("Category", "WritesToApi")]
	public async Task CreateAndQueryIssueWithComplexCustomFields(JiraClient jira)
	{
		var dateTime = new DateTime(2016, 11, 11, 11, 11, 0);
		var dateTimeStr = dateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffzzz");
		dateTimeStr = dateTimeStr.Remove(dateTimeStr.LastIndexOf(':'), 1);

		var summaryValue = "Test issue with lots of custom fields (Created)" + RandomNumberGenerator.GetInt32(int.MaxValue);
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
		issue.CustomFields.AddArray("Custom Labels Field", ["label1", "label2"]);
		issue.CustomFields.AddArray("Custom Multi Group Field", ["jira-developers", "jira-users"]);
		issue.CustomFields.AddArray("Custom Multi Select Field", ["option1", "option2"]);
		issue.CustomFields.AddArray("Custom Multi User Field", ["admin", "test"]);
		issue.CustomFields.AddArray("Custom Checkboxes Field", ["option1", "option2"]);
		issue.CustomFields.AddArray("Custom Multi Version Field", ["2.0", "3.0"]);
		issue.CustomFields.AddCascadingSelectField("Custom Cascading Select Field", "Option2", "Option2.2");

		await issue.SaveChangesAsync(CancellationToken);

		var newIssue = await jira.Issues.GetIssueAsync(issue.Key.Value, CancellationToken);

		newIssue["Custom Text Field"].Should().Be("My new value");
		newIssue["Custom Date Field"].Should().Be("2015-10-03");
		newIssue["Custom User Field"].Should().Be("admin");
		newIssue["Custom Select Field"].Should().Be("Blue");
		newIssue["Custom Group Field"].Should().Be("jira-users");
		newIssue["Custom Project Field"].Should().Be("TST");
		newIssue["Custom Version Field"].Should().Be("1.0");
		newIssue["Custom Radio Field"].Should().Be("option1");
		newIssue["Custom Number Field"].Should().Be("12.34");
		newIssue.CustomFields.GetAs<JiraUser>("Custom User Field").Email.Should().Be("admin@example.com");

		var serverDate = DateTime.Parse(newIssue["Custom DateTime Field"].Value);
		serverDate.Should().Be(dateTime);

		newIssue.CustomFields["Custom Labels Field"].Values.Should().BeEquivalentTo(new[] { "label1", "label2" });
		newIssue.CustomFields["Custom Multi Group Field"].Values.Should().BeEquivalentTo(new[] { "jira-developers", "jira-users" });
		newIssue.CustomFields["Custom Multi Select Field"].Values.Should().BeEquivalentTo(new[] { "option1", "option2" });
		newIssue.CustomFields["Custom Multi User Field"].Values.Should().BeEquivalentTo(new[] { "admin", "test" });
		newIssue.CustomFields["Custom Checkboxes Field"].Values.Should().BeEquivalentTo(new[] { "option1", "option2" });
		newIssue.CustomFields["Custom Multi Version Field"].Values.Should().BeEquivalentTo(new[] { "2.0", "3.0" });

		var users = newIssue.CustomFields.GetAs<JiraUser[]>("Custom Multi User Field");
		users.Should().Contain(u => u.Email == "test@qa.com");

		var cascadingSelect = newIssue.CustomFields.GetCascadingSelectField("Custom Cascading Select Field");
		cascadingSelect.ParentOption.Should().Be("Option2");
		cascadingSelect.ChildOption.Should().Be("Option2.2");
		cascadingSelect.Name.Should().Be("Custom Cascading Select Field");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
    [Trait("Category", "WritesToApi")]
	public async Task CanClearValueOfCustomField(JiraClient jira)
	{
		var summaryValue = "Test issue " + RandomNumberGenerator.GetInt32(int.MaxValue);
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		issue["Custom Text Field"] = "My new value";
		issue["Custom Date Field"] = "2015-10-03";
		issue["Custom Select Field"] = "Blue";
		await issue.SaveChangesAsync(CancellationToken);

		var newIssue = await jira.Issues.GetIssueAsync(issue.Key.Value, CancellationToken);
        newIssue["Custom Text Field"].Should().Be("My new value");
		newIssue["Custom Date Field"].Should().Be("2015-10-03");
		newIssue["Custom Text Field"] = null;
		newIssue["Custom Date Field"] = null;
		newIssue["Custom Select Field"] = null;
		await newIssue.SaveChangesAsync(CancellationToken);

		var updatedIssue = await jira.Issues.GetIssueAsync(issue.Key.Value, CancellationToken);

		updatedIssue["Custom Text Field"].Should().BeNull();
		updatedIssue["Custom Date Field"].Should().BeNull();
		updatedIssue["Custom Select Field"].Should().BeNull();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
 [Trait("Category", "WritesToApi")]
	public async Task CreateAndUpdateIssueWithComplexCustomFields(JiraClient jira)
	{
		var dateTime = new DateTime(2016, 11, 11, 11, 11, 0);
		var dateTimeStr = dateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffzzz");
		dateTimeStr = dateTimeStr.Remove(dateTimeStr.LastIndexOf(':'), 1);
		var summaryValue = "Test issue with lots of custom fields (Created)" + RandomNumberGenerator.GetInt32(int.MaxValue);

		// Create issue with no custom fields set
		var issue = new Issue(jira, "TST")
		{
			Type = "1",
			Summary = summaryValue,
			Assignee = "admin"
		};

		await issue.SaveChangesAsync(CancellationToken);

		// Retrieve the issue, set all custom fields and save the changes.
		var newIssue = await jira.Issues.GetIssueAsync(issue.Key.Value, CancellationToken);

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
		newIssue.CustomFields.AddArray("Custom Labels Field", ["label1", "label2"]);
		newIssue.CustomFields.AddArray("Custom Multi Group Field", ["jira-developers", "jira-users"]);
		newIssue.CustomFields.AddArray("Custom Multi Select Field", ["option1", "option2"]);
		newIssue.CustomFields.AddArray("Custom Multi User Field", ["admin", "test"]);
		newIssue.CustomFields.AddArray("Custom Checkboxes Field", ["option1", "option2"]);
		newIssue.CustomFields.AddArray("Custom Multi Version Field", ["2.0", "3.0"]);
		newIssue.CustomFields.AddCascadingSelectField("Custom Cascading Select Field", "Option2", "Option2.2");

		await newIssue.SaveChangesAsync(CancellationToken);

		// Retrieve the issue again and verify fields
		var updatedIssue = await jira.Issues.GetIssueAsync(issue.Key.Value, CancellationToken);

        updatedIssue["Custom Text Field"].Should().Be("My new value");
		updatedIssue["Custom Date Field"].Should().Be("2015-10-03");
		updatedIssue["Custom User Field"].Should().Be("admin");
		updatedIssue["Custom Select Field"].Should().Be("Blue");
		updatedIssue["Custom Group Field"].Should().Be("jira-users");
		updatedIssue["Custom Project Field"].Should().Be("TST");
		updatedIssue["Custom Version Field"].Should().Be("1.0");
		updatedIssue["Custom Radio Field"].Should().Be("option1");
		updatedIssue["Custom Number Field"].Should().Be("1234");

		var serverDate = DateTime.Parse(updatedIssue["Custom DateTime Field"].Value);
		serverDate.Should().Be(dateTime);

        updatedIssue.CustomFields["Custom Labels Field"].Values.Should().Equal("label1", "label2");
		updatedIssue.CustomFields["Custom Multi Group Field"].Values.Should().Equal("jira-developers", "jira-users");
		updatedIssue.CustomFields["Custom Multi Select Field"].Values.Should().Equal("option1", "option2");
		updatedIssue.CustomFields["Custom Multi User Field"].Values.Should().Equal("admin", "test");
		updatedIssue.CustomFields["Custom Checkboxes Field"].Values.Should().Equal("option1", "option2");
		updatedIssue.CustomFields["Custom Multi Version Field"].Values.Should().Equal("2.0", "3.0");

		var cascadingSelect = updatedIssue.CustomFields.GetCascadingSelectField("Custom Cascading Select Field");
		cascadingSelect.ParentOption.Should().Be("Option2");
		cascadingSelect.ChildOption.Should().Be("Option2.2");
		cascadingSelect.Name.Should().Be("Custom Cascading Select Field");

		// Update custom fields again and save
		updatedIssue["Custom Text Field"] = "My newest value";
		updatedIssue["Custom Date Field"] = "2019-10-03";
		updatedIssue["Custom Number Field"] = "9999";
		updatedIssue.CustomFields["Custom Labels Field"].Values = ["label3"];
		await updatedIssue.SaveChangesAsync(CancellationToken);

		// Retrieve the issue one last time and verify custom fields.
		var updatedIssue2 = await jira.Issues.GetIssueAsync(issue.Key.Value, CancellationToken);
     updatedIssue["Custom Text Field"].Should().Be("My newest value");
		updatedIssue["Custom Date Field"].Should().Be("2019-10-03");
		updatedIssue2["Custom Number Field"].Should().Be("9999");
		updatedIssue.CustomFields["Custom Labels Field"].Values.Should().Equal("label3");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
  [Trait("Category", "WritesToApi")]
	public async Task CreateAndQuerySprintName(JiraClient jira)
	{
		var issue = new Issue(jira, "SCRUM")
		{
			Type = "Bug",
			Summary = "Test issue with sprint" + RandomNumberGenerator.GetInt32(int.MaxValue),
			Assignee = "admin"
		};
		// Set the sprint by id
		issue["Sprint"] = "1";
		await issue.SaveChangesAsync(CancellationToken);

		// Get the sprint by name
		var newIssue = await jira.Issues.GetIssueAsync(issue.Key.Value, CancellationToken);
       newIssue["Sprint"].Should().Be("Sprint 1");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
  [Trait("Category", "WritesToApi")]
	public async Task UpdateAndQuerySprintName(JiraClient jira)
	{
		var issue = new Issue(jira, "SCRUM")
		{
			Type = "Bug",
			Summary = "Test issue with sprint" + RandomNumberGenerator.GetInt32(int.MaxValue),
			Assignee = "admin"
		};
		await issue.SaveChangesAsync(CancellationToken);
		issue["Sprint"].Should().BeNull();

		// Set the sprint by id
		issue["Sprint"] = "1";
		await issue.SaveChangesAsync(CancellationToken);

		// Get the sprint by name
		var newIssue = await jira.Issues.GetIssueAsync(issue.Key.Value, CancellationToken);
       newIssue["Sprint"].Should().Be("Sprint 1");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
    [Trait("Category", "WritesToApi")]
	public async Task CanUpdateIssueWithoutModifyingCustomFields(JiraClient jira)
	{
		var issue = new Issue(jira, "SCRUM")
		{
			Type = "Bug",
			Summary = "Test issue with sprint" + RandomNumberGenerator.GetInt32(int.MaxValue),
			Assignee = "admin"
		};
		issue["Sprint"] = "1";
		await issue.SaveChangesAsync(CancellationToken);
      issue["Sprint"].Should().Be("Sprint 1");

		issue.Summary += " (Updated)";
		await issue.SaveChangesAsync(CancellationToken);
      issue["Sprint"].Should().Be("Sprint 1");
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
    [Trait("Category", "WritesToApi")]
	public async Task ThrowsErrorWhenSettingSprintByName(JiraClient jira)
	{
		var issue = new Issue(jira, "SCRUM")
		{
			Type = "Bug",
			Summary = "Test issue with sprint" + RandomNumberGenerator.GetInt32(int.MaxValue),
			Assignee = "admin"
		};

		// Set the sprint by name
		issue["Sprint"] = "Sprint 1";

		try
		{
			await issue.SaveChangesAsync(CancellationToken);
			throw new Exception("Method did not throw exception");
		}
		catch (AggregateException ex)
		{
			ex.Flatten().InnerException.Message.Should().Contain("Number value expected as the Sprint id");
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



