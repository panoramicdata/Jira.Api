using Jira.Api.Linq;
using System.Globalization;

namespace Jira.Api.Test;

public class JqlExpressionTranslatorTest
{
	private JqlExpressionVisitor _translator = null!;

	private JiraQueryable<Issue> CreateQueryable()
	{

		_translator = new JqlExpressionVisitor();

		var jira = JiraClient.CreateRestClient("http://foo");
		var issues = new Mock<IIssueService>();
		var provider = new JiraQueryProvider(_translator, issues.Object);

		issues.SetupIssues(jira, new RemoteIssue());

		return new JiraQueryable<Issue>(provider);
	}

	[Fact]
	public void EqualsOperatorForNonString()
	{
		var queryable = CreateQueryable();

		var issues = (from i in queryable
					  where i.Votes == 5
					  select i).ToArray();

		_translator.Jql.Should().Be("Votes = 5");
	}

	[Fact]
	public void EqualsOperatorForStringWithFuzzyEquality()
	{
		var queryable = CreateQueryable();

		var issues = (from i in queryable
					  where i.Summary == "Foo"
					  select i).ToArray();

		_translator.Jql.Should().Be("Summary ~ \"Foo\"");
	}

	[Fact]
	public void EqualsOperatorForString()
	{
		var queryable = CreateQueryable();

		var issues = (from i in queryable
					  where i.Assignee == "Foo"
					  select i).ToArray();

		_translator.Jql.Should().Be("Assignee = \"Foo\"");
	}

	[Fact]
	public void NotEqualsOperatorForNonString()
	{
		var queryable = CreateQueryable();

		var issues = (from i in queryable
					  where i.Votes != 5
					  select i).ToArray();

		_translator.Jql.Should().Be("Votes != 5");
	}

	[Fact]
	public void NotEqualsOperatorForStringWithFuzzyEquality()
	{
		var queryable = CreateQueryable();

		var issues = (from i in queryable
					  where i.Summary != "Foo"
					  select i).ToArray();

		_translator.Jql.Should().Be("Summary !~ \"Foo\"");
	}

	[Fact]
	public void NotEqualsOperatorForString()
	{
		var queryable = CreateQueryable();

		var issues = (from i in queryable
					  where i.Assignee != "Foo"
					  select i).ToArray();

		_translator.Jql.Should().Be("Assignee != \"Foo\"");
	}

	[Fact]
	public void GreaterThanOperator()
	{
		var queryable = CreateQueryable();

		var issues = (from i in queryable
					  where i.Votes > 5
					  select i).ToArray();

		_translator.Jql.Should().Be("Votes > 5");
	}

	[Fact]
	public void GreaterThanEqualsOperator()
	{
		var queryable = CreateQueryable();

		var issues = (from i in queryable
					  where i.Votes >= 5
					  select i).ToArray();

		_translator.Jql.Should().Be("Votes >= 5");
	}

	[Fact]
	public void LessThanOperator()
	{
		var queryable = CreateQueryable();

		var issues = (from i in queryable
					  where i.Votes < 5
					  select i).ToArray();

		_translator.Jql.Should().Be("Votes < 5");
	}

	[Fact]
	public void LessThanOrEqualsOperator()
	{
		var queryable = CreateQueryable();

		var issues = (from i in queryable
					  where i.Votes <= 5
					  select i).ToArray();

		_translator.Jql.Should().Be("Votes <= 5");
	}

	[Fact]
	public void AndKeyWord()
	{
		var queryable = CreateQueryable();

		var issues = (from i in queryable
					  where i.Votes > 5 && i.Votes < 10
					  select i).ToArray();

		_translator.Jql.Should().Be("(Votes > 5 and Votes < 10)");
	}

	[Fact]
	public void OrKeyWord()
	{
		var queryable = CreateQueryable();

		var issues = (from i in queryable
					  where i.Votes > 5 || i.Votes < 10
					  select i).ToArray();

		_translator.Jql.Should().Be("(Votes > 5 or Votes < 10)");
	}

	[Fact]
	public void AssociativeGrouping()
	{
		var queryable = CreateQueryable();

		var issues = (from i in queryable
					  where i.Votes > 5 && (i.Votes < 10 || i.Votes == 20)
					  select i).ToArray();

		_translator.Jql.Should().Be("(Votes > 5 and (Votes < 10 or Votes = 20))");
	}

	[Fact]
	public void IsOperatorForEmptyString()
	{
		var queryable = CreateQueryable();

		var issues = (from i in queryable
					  where i.Summary == ""
					  select i).ToArray();

		_translator.Jql.Should().Be("Summary is empty");
	}

	[Fact]
	public void IsNotOperatorForEmptyString()
	{
		var queryable = CreateQueryable();

		var issues = (from i in queryable
					  where i.Summary != ""
					  select i).ToArray();

		_translator.Jql.Should().Be("Summary is not empty");
	}

	[Fact]
	public void IsOperatorForNull()
	{
		var queryable = CreateQueryable();

		var issues = (from i in queryable
					  where i.Summary == null
					  select i).ToArray();

		_translator.Jql.Should().Be("Summary is null");
	}

	[Fact]
	public void GreaterThanOperatorWhenUsingComparableFieldWithString()
	{
		var queryable = CreateQueryable();

		var issues = (from i in queryable
					  where i.Priority > "foo"
					  select i).ToArray();

		_translator.Jql.Should().Be("Priority > \"foo\"");
	}

	[Fact]
	public void EqualsOperatorWhenUsingComparableFieldWithString()
	{
		var queryable = CreateQueryable();

		var issues = (from i in queryable
					  where i.Priority == "foo"
					  select i).ToArray();

		_translator.Jql.Should().Be("Priority = \"foo\"");
	}

	[Fact]
	public void OrderBy()
	{
		var queryable = CreateQueryable();

		var issues = (from i in queryable
					  where i.Priority == "1"
					  orderby i.Created
					  select i).ToArray();

		_translator.Jql.Should().Be("Priority = \"1\" order by Created asc");
	}

	[Fact]
	public void OrderByDescending()
	{
		var queryable = CreateQueryable();

		var issues = (from i in queryable
					  where i.Priority == "1"
					  orderby i.Created descending
					  select i).ToArray();

		_translator.Jql.Should().Be("Priority = \"1\" order by Created desc");
	}

	[Fact]
	public void MultipleOrderBys()
	{
		var queryable = CreateQueryable();

		var issues = (from i in queryable
					  where i.Priority == "1"
					  orderby i.Created, i.DueDate
					  select i).ToArray();

		_translator.Jql.Should().Be("Priority = \"1\" order by Created asc, DueDate asc");
	}

	[Fact]
	public void MultipleOrderByDescending()
	{
		var queryable = CreateQueryable();

		var issues = (from i in queryable
					  where i.Priority == "1"
					  orderby i.Created, i.DueDate descending
					  select i).ToArray();

		_translator.Jql.Should().Be("Priority = \"1\" order by Created asc, DueDate desc");
	}

	[Fact]
	public void NewDateTime()
	{
		var queryable = CreateQueryable();

		var issues = (from i in queryable
					  where i.Created > new DateTime(2011, 1, 1)
					  select i).ToArray();

		_translator.Jql.Should().Be("Created > \"2011/01/01\"");
	}

	[Fact]
	public void MultipleDateTimes()
	{
		var queryable = CreateQueryable();

		var issues = (from i in queryable
					  where i.Created > new DateTime(2011, 1, 1) && i.Created < new DateTime(2012, 1, 1)
					  select i).ToArray();

		_translator.Jql.Should().Be("(Created > \"2011/01/01\" and Created < \"2012/01/01\")");
	}

	[Fact]
	public void LocalStringVariables()
	{
		var queryable = CreateQueryable();
		var user = "farmas";

		var issues = (from i in queryable
					  where i.Assignee == user
					  select i).ToArray();

		_translator.Jql.Should().Be("Assignee = \"farmas\"");
	}

	[Fact]
	public void LocalDateVariables()
	{
		var queryable = CreateQueryable();
		var date = new DateTime(2011, 1, 1);

		var issues = (from i in queryable
					  where i.Created > date
					  select i).ToArray();

		_translator.Jql.Should().Be("Created > \"2011/01/01\"");
	}

	[Fact]
	public void DateTimeWithLiteralString()
	{
		var queryable = CreateQueryable();
		var date = new DateTime(2011, 1, 1);

		var issues = (from i in queryable
					  where i.Created > new LiteralDateTime(date.ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture))
					  select i).ToArray();

		_translator.Jql.Should().Be("Created > \"2011/01/01 00:00\"");
	}

	[Fact]
	// https://bitbucket.org/farmas/atlassian.net-sdk/issue/31
	public void DateTimeFormattedAsEnUs()
	{
		var currentCulture = Thread.CurrentThread.CurrentCulture;

		try
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
			var queryable = CreateQueryable();
			var date = new DateTime(2011, 1, 1);

			var issues = (from i in queryable
						  where i.Created > date
						  select i).ToArray();

			_translator.Jql.Should().Be("Created > \"2011/01/01\"");
		}
		finally
		{
			Thread.CurrentThread.CurrentCulture = currentCulture;
		}
	}

	[Fact]
	public void DateNow()
	{
		var queryable = CreateQueryable();

		var issues = (from i in queryable
					  where i.Created > DateTime.Now.Date
					  select i).ToArray();

		_translator.Jql.Should().Be("Created > \"" + DateTime.Now.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture) + "\"");
	}

	[Fact]
	public void DateTimeNow()
	{
		var queryable = CreateQueryable();

		var issues = (from i in queryable
					  where i.Created > DateTime.Now
					  select i).ToArray();

		_translator.Jql.Should().Be("Created > \"" + DateTime.Now.ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture) + "\"");
	}

	[Fact]
	public void TakeWithConstant()
	{
	var queryable = CreateQueryable();

	var issues = (from i in queryable
		where i.Assignee == "foo"
		select i).Take(50).ToArray();

	_translator.NumberOfResults.Should().Be(50);
	}

	[Fact]
	public void SkipWithConstant()
	{
	var queryable = CreateQueryable();

	var issues = (from i in queryable
		where i.Assignee == "foo"
		select i).Skip(25).Take(50).ToArray();

	_translator.SkipResults.Should().Be(25);
	}

	[Fact]
	public void SkipAndTakeShouldResetOnEveryProcessOperation()
	{
		var queryable = CreateQueryable();

		var issues = (from i in queryable
					  where i.Assignee == "foo"
					  select i).Skip(25).Take(50).ToArray();

		var issues2 = (from i in queryable
					   where i.Assignee == "foo"
					   select i).ToArray();

		_translator.SkipResults.Should().BeNull();
		_translator.NumberOfResults.Should().BeNull();
	}

	[Fact]
	public void TakeWithLocalVariable()
	{
		var queryable = CreateQueryable();
		var take = 100;

		var issues = (from i in queryable
					  where i.Assignee == "foo"
					  select i).Take(take).ToArray();

		_translator.NumberOfResults.Should().Be(100);
	}

	[Fact]
	public void VersionsEqual()
	{
		var queryable = CreateQueryable();
		var issues = (from i in queryable
					  where i.FixVersions == "1.0" && i.AffectsVersions == "2.0"
					  select i).ToArray();

		_translator.Jql.Should().Be("(FixVersion = \"1.0\" and AffectedVersion = \"2.0\")");
	}

	[Fact]
	public void ComponentEqual()
	{
		var queryable = CreateQueryable();
		var issues = (from i in queryable
					  where i.Components == "foo"
					  select i).ToArray();

		_translator.Jql.Should().Be("component = \"foo\"");
	}

	[Fact]
	public void VersionsNotEqual()
	{
		var queryable = CreateQueryable();
		var issues = (from i in queryable
					  where i.FixVersions != "1.0" && i.AffectsVersions != "2.0"
					  select i).ToArray();

		_translator.Jql.Should().Be("(FixVersion != \"1.0\" and AffectedVersion != \"2.0\")");
	}

	[Fact]
	public void ComponentNotEqual()
	{
		var queryable = CreateQueryable();
		var issues = (from i in queryable
					  where i.Components != "foo"
					  select i).ToArray();

		_translator.Jql.Should().Be("component != \"foo\"");
	}

	[Fact]
	public void CanUseLiteralMatchOnMemberProperties()
	{
		var queryable = CreateQueryable();
		var issues = (from i in queryable
					  where i.Summary == new LiteralMatch("Literal Summary") && i.Description == new LiteralMatch("Literal Description")
					  select i).ToArray();

		_translator.Jql.Should().Be("(Summary = \"Literal Summary\" and Description = \"Literal Description\")");
	}

	[Fact]
	public void CustomFieldEqual()
	{
		var queryable = CreateQueryable();
		var issues = (from i in queryable
					  where i["Foo"]! == "foo" && i["Bar"]! == new DateTime(2012, 1, 1) && i["Baz"]! == new LiteralMatch("baz")
					  select i).ToArray();

		_translator.Jql.Should().Be("((\"Foo\" ~ \"foo\" and \"Bar\" = \"2012/01/01\") and \"Baz\" = \"baz\")");
	}

	[Fact]
	public void CustomFieldNotEqual()
	{
		var queryable = CreateQueryable();
		var issues = (from i in queryable
					  where i["Foo"]! != "foo" && i["Bar"]! != new DateTime(2012, 1, 1) && i["Baz"]! != new LiteralMatch("baz")
					  select i).ToArray();

		_translator.Jql.Should().Be("((\"Foo\" !~ \"foo\" and \"Bar\" != \"2012/01/01\") and \"Baz\" != \"baz\")");
	}

	[Fact]
	public void CustomFieldGreaterThan()
	{
		var queryable = CreateQueryable();
		var issues = (from i in queryable
					  where i["Foo"]! > "foo" && i["Bar"]! > new DateTime(2012, 1, 1)
					  select i).ToArray();

		_translator.Jql.Should().Be("(\"Foo\" > \"foo\" and \"Bar\" > \"2012/01/01\")");
	}

	[Fact]
	public void MultipleSeparateWheres()
	{
		var queryable = CreateQueryable();

		var issues = from i in queryable
					 where i.Votes == 5
					 select i;

		issues = from i in issues
				 where i.Status == "Open" && i.Assignee == "admin"
				 select i;

		issues = from i in issues
				 where i.Priority == "1"
				 select i;

		var issuesArray = issues.ToArray();

		_translator.Jql.Should().Be("Votes = 5 and (Status = \"Open\" and Assignee = \"admin\") and Priority = \"1\"");
	}
}

