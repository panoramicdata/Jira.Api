using Jira.Api.Linq;
using System.Globalization;

namespace Jira.Api.Test;

public class JqlExpressionTranslatorTest
{
	private JqlExpressionVisitor _translator = null!;

	public static TheoryData<Func<IQueryable<Issue>, IQueryable<Issue>>, string> JqlCases => new()
	{
		{ q => q.Where(i => i.Votes == 5), "Votes = 5" },
		{ q => q.Where(i => i.Summary == "Foo"), "Summary ~ \"Foo\"" },
		{ q => q.Where(i => i.Assignee == "Foo"), "Assignee = \"Foo\"" },
		{ q => q.Where(i => i.Votes != 5), "Votes != 5" },
		{ q => q.Where(i => i.Summary != "Foo"), "Summary !~ \"Foo\"" },
		{ q => q.Where(i => i.Assignee != "Foo"), "Assignee != \"Foo\"" },
		{ q => q.Where(i => i.Votes > 5), "Votes > 5" },
		{ q => q.Where(i => i.Votes >= 5), "Votes >= 5" },
		{ q => q.Where(i => i.Votes < 5), "Votes < 5" },
		{ q => q.Where(i => i.Votes <= 5), "Votes <= 5" },
		{ q => q.Where(i => i.Votes > 5 && i.Votes < 10), "(Votes > 5 and Votes < 10)" },
		{ q => q.Where(i => i.Votes > 5 || i.Votes < 10), "(Votes > 5 or Votes < 10)" },
		{ q => q.Where(i => i.Votes > 5 && (i.Votes < 10 || i.Votes == 20)), "(Votes > 5 and (Votes < 10 or Votes = 20))" },
		{ q => q.Where(i => i.Summary == ""), "Summary is empty" },
		{ q => q.Where(i => i.Summary != ""), "Summary is not empty" },
		{ q => q.Where(i => i.Summary == null), "Summary is null" },
		{ q => q.Where(i => i.Priority > "foo"), "Priority > \"foo\"" },
		{ q => q.Where(i => i.Priority == "foo"), "Priority = \"foo\"" },
		{ q => q.Where(i => i.Priority == "1").OrderBy(i => i.Created), "Priority = \"1\" order by Created asc" },
		{ q => q.Where(i => i.Priority == "1").OrderByDescending(i => i.Created), "Priority = \"1\" order by Created desc" },
		{ q => q.Where(i => i.Priority == "1").OrderBy(i => i.Created).ThenBy(i => i.DueDate), "Priority = \"1\" order by Created asc, DueDate asc" },
		{ q => q.Where(i => i.Priority == "1").OrderBy(i => i.Created).ThenByDescending(i => i.DueDate), "Priority = \"1\" order by Created asc, DueDate desc" },
		{ q => q.Where(i => i.Created > new DateTime(2011, 1, 1)), "Created > \"2011/01/01\"" },
		{ q => q.Where(i => i.Created > new DateTime(2011, 1, 1) && i.Created < new DateTime(2012, 1, 1)), "(Created > \"2011/01/01\" and Created < \"2012/01/01\")" },
		{ q => q.Where(i => i.Created > new LiteralDateTime("2011/01/01 00:00")), "Created > \"2011/01/01 00:00\"" },
		{ q => q.Where(i => i.FixVersions == "1.0" && i.AffectsVersions == "2.0"), "(FixVersion = \"1.0\" and AffectedVersion = \"2.0\")" },
		{ q => q.Where(i => i.Components == "foo"), "component = \"foo\"" },
		{ q => q.Where(i => i.FixVersions != "1.0" && i.AffectsVersions != "2.0"), "(FixVersion != \"1.0\" and AffectedVersion != \"2.0\")" },
		{ q => q.Where(i => i.Components != "foo"), "component != \"foo\"" },
		{ q => q.Where(i => i.Summary == new LiteralMatch("Literal Summary") && i.Description == new LiteralMatch("Literal Description")), "(Summary = \"Literal Summary\" and Description = \"Literal Description\")" },
		{ q => q.Where(i => i["Foo"]! == "foo" && i["Bar"]! == new DateTime(2012, 1, 1) && i["Baz"]! == new LiteralMatch("baz")), "((\"Foo\" ~ \"foo\" and \"Bar\" = \"2012/01/01\") and \"Baz\" = \"baz\")" },
		{ q => q.Where(i => i["Foo"]! != "foo" && i["Bar"]! != new DateTime(2012, 1, 1) && i["Baz"]! != new LiteralMatch("baz")), "((\"Foo\" !~ \"foo\" and \"Bar\" != \"2012/01/01\") and \"Baz\" != \"baz\")" },
		{ q => q.Where(i => i["Foo"]! > "foo" && i["Bar"]! > new DateTime(2012, 1, 1)), "(\"Foo\" > \"foo\" and \"Bar\" > \"2012/01/01\")" },
		{ q => q.Where(i => i.Votes == 5).Where(i => i.Status == "Open" && i.Assignee == "admin").Where(i => i.Priority == "1"), "Votes = 5 and (Status = \"Open\" and Assignee = \"admin\") and Priority = \"1\"" }
	};

	[Theory]
	[MemberData(nameof(JqlCases))]
	public void Query_TranslatesToJql(Func<IQueryable<Issue>, IQueryable<Issue>> query, string expectedJql)
	{
		_ = query(CreateQueryable()).ToArray();

		_translator.Jql.Should().Be(expectedJql);
	}

	[Fact]
	public void LocalVariables_AreEvaluated()
	{
		var user = "farmas";
		var date = new DateTime(2011, 1, 1);

		_ = CreateQueryable().Where(i => i.Assignee == user && i.Created > date).ToArray();

		_translator.Jql.Should().Be("(Assignee = \"farmas\" and Created > \"2011/01/01\")");
	}

	[Fact]
	public void DateTimeFormattedAsEnUs()
	{
		var currentCulture = Thread.CurrentThread.CurrentCulture;

		try
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
			var date = new DateTime(2011, 1, 1);

			_ = CreateQueryable().Where(i => i.Created > date).ToArray();

			_translator.Jql.Should().Be("Created > \"2011/01/01\"");
		}
		finally
		{
			Thread.CurrentThread.CurrentCulture = currentCulture;
		}
	}

	[Theory]
	[InlineData(true, "yyyy/MM/dd")]
	[InlineData(false, "yyyy/MM/dd HH:mm")]
	public void CurrentDateTime_IsFormatted(bool dateOnly, string format)
	{
		var queryable = CreateQueryable();

		_ = (dateOnly ? queryable.Where(i => i.Created > DateTime.Now.Date) : queryable.Where(i => i.Created > DateTime.Now)).ToArray();

		_translator.Jql.Should().StartWith("Created > \"").And.EndWith("\"");
		DateTime.ParseExact(_translator.Jql[11..^1], format, CultureInfo.InvariantCulture).Should().BeCloseTo(DateTime.Now, TimeSpan.FromDays(2));
	}

	[Theory]
	[InlineData(0, 50, null, 50)]
	[InlineData(25, 50, 25, 50)]
	public void SkipAndTake_AreCaptured(int skip, int take, int? expectedSkip, int expectedTake)
	{
		var queryable = CreateQueryable().Where(i => i.Assignee == "foo");

		_ = (skip > 0 ? queryable.Skip(skip).Take(take) : queryable.Take(take)).ToArray();

		_translator.SkipResults.Should().Be(expectedSkip);
		_translator.NumberOfResults.Should().Be(expectedTake);
	}

	[Fact]
	public void SkipAndTake_ResetOnEveryProcessOperation()
	{
		var queryable = CreateQueryable();

		_ = queryable.Where(i => i.Assignee == "foo").Skip(25).Take(50).ToArray();
		_ = queryable.Where(i => i.Assignee == "foo").ToArray();

		_translator.SkipResults.Should().BeNull();
		_translator.NumberOfResults.Should().BeNull();
	}

	[Fact]
	public void TakeWithLocalVariable()
	{
		var take = 100;

		_ = CreateQueryable().Where(i => i.Assignee == "foo").Take(take).ToArray();

		_translator.NumberOfResults.Should().Be(100);
	}

	private JiraQueryable<Issue> CreateQueryable()
	{
		_translator = new JqlExpressionVisitor();

		var jira = JiraClient.CreateRestClient("http://foo");
		var issues = new Mock<IIssueService>();
		var provider = new JiraQueryProvider(_translator, issues.Object);

		issues.SetupIssues(jira, new RemoteIssue());

		return new JiraQueryable<Issue>(provider);
	}
}
