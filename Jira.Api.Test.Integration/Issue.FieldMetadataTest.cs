using System.Text.RegularExpressions;

namespace Jira.Api.Test.Integration;

[Trait("Category", "WritesToApi")]
public class IssueFieldMetadataTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task TestNonCustomFieldOption(JiraClient jira)
	{
		// prepare
		Issue iss = await jira.Issues.GetIssueAsync("TST-1", CancellationToken);

		// exercise
		var issueFields = await iss.GetIssueFieldsEditMetadataAsync(CancellationToken);

		IssueFieldEditMetadata customRadioField = issueFields["Component/s"];
		customRadioField.IsCustom.Should().BeFalse();
	}

	[Theory]
	[ClassData(typeof(JiraProvider))]
	public async Task TestCustomFieldOptions(JiraClient jira)
	{
		// prepare
		Issue iss = await jira.Issues.GetIssueAsync("TST-1", CancellationToken);

		// exercise
		var issueFields = await iss.GetIssueFieldsEditMetadataAsync(CancellationToken);

		//assert: IssueFieldEditMetadata of issue
		(issueFields.Count >= 34).Should().BeTrue();
		IssueFieldEditMetadata customRadioField = issueFields["Custom Radio Field"];
		customRadioField.IsCustom.Should().BeTrue();
		customRadioField.Name.Should().Be("Custom Radio Field");
		customRadioField.IsRequired.Should().BeFalse();
		customRadioField.Operations.Should().Contain(IssueFieldEditMetadataOperation.SET);
		customRadioField.Operations.Should().ContainSingle();
		customRadioField.Schema.Type.Should().Be("option");
		customRadioField.Schema.Custom.Should().Be("com.atlassian.jira.plugin.system.customfieldtypes:radiobuttons");
      customRadioField.Schema.CustomId.Should().Be(10307);

		// assert: allowed values
		// warning : AllowedValues on IssueFieldEditMetadata could be various kind of objects. One can determine the kind of object
		// by looking at it's properties. Issue TST-1 and it's field "Custom Radio Field" used for this test has
		// AllowedValues elements are objects of type CustomFieldOption
		var options = customRadioField.AllowedValuesAs<IssueCustomFieldTest.IssueFieldMetadataCustomFieldOption>();
		IssueCustomFieldTest.IssueFieldMetadataCustomFieldOption? option1 = options.FirstOrDefault(x => x.Value == "option1");
		IssueCustomFieldTest.IssueFieldMetadataCustomFieldOption? option2 = options.FirstOrDefault(x => x.Value == "option2");
		IssueCustomFieldTest.IssueFieldMetadataCustomFieldOption? option3 = options.FirstOrDefault(x => x.Value == "option3");
		option1.Should().NotBeNull();
		option2.Should().NotBeNull();
		option3.Should().NotBeNull();
		AssertCustomFieldOption(option1, 10103, "option1", @".*/rest/api/2/customFieldOption/10103");
		AssertCustomFieldOption(option2, 10104, "option2", @".*/rest/api/2/customFieldOption/10104");
		AssertCustomFieldOption(option3, 10105, "option3", @".*/rest/api/2/customFieldOption/10105");
	}

	private static void AssertCustomFieldOption(IssueCustomFieldTest.IssueFieldMetadataCustomFieldOption option, int id, string value, string selfRegex)
	{
		option.Should().NotBeNull();
        option.Id.Should().Be(id);
		option.Value.Should().Be(value);
		Regex regex = new(selfRegex);
		regex.Match(option.Self).Success.Should().BeTrue();
	}
}



