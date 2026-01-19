#nullable enable
using AwesomeAssertions;

namespace Jira.Api.Test;

public class ComparableStringTest
{
	[Fact]
	public void RefereceIsNull_EqualsOperators()
	{
		ComparableString? field = null;
		(field is null).Should().BeTrue();
		(field is not null).Should().BeFalse();
	}

	public class WithDate
	{
		[Fact]
		public void StringEqualsOperator()
		{
			(new ComparableString("2012/05/01") == new DateTime(2012, 4, 1)).Should().BeFalse();
			(new ComparableString("2012/04/01") == new DateTime(2012, 4, 1)).Should().BeTrue();
		}

		[Fact]
		public void StringNotEqualsOperator()
		{
			(new ComparableString("2012/05/01") != new DateTime(2012, 4, 1)).Should().BeTrue();
			(new ComparableString("2012/04/01") != new DateTime(2012, 4, 1)).Should().BeFalse();
		}

		[Fact]
		public void StringGreaterThanOperator()
		{
			(new ComparableString("2012/01/10") > new DateTime(2012, 1, 1)).Should().BeTrue();
		}

		[Fact]
		public void StringLessThanOperator()
		{
			(new ComparableString("2012/01/10") < new DateTime(2012, 1, 11)).Should().BeTrue();
		}

		[Fact]
		public void StringLessThanOrEqualsOperator()
		{
			(new ComparableString("2012/01/10") <= new DateTime(2012, 1, 10)).Should().BeTrue();
		}
	}

	public class WithString
	{
		[Fact]
		public void StringEqualsOperator()
		{
			(new ComparableString("bar") == "foo").Should().BeFalse();
			(new ComparableString("foo") == "foo").Should().BeTrue();
		}

		[Fact]
		public void StringNotEqualsOperator()
		{
			(new ComparableString("bar") != "foo").Should().BeTrue();
			(new ComparableString("foo") != "foo").Should().BeFalse();
		}

		[Fact]
		public void StringGreaterThanOperator()
		{
			(new ComparableString("TST-23") > "TST-1").Should().BeTrue();
		}

		[Fact]
		public void StringLessThanOperator()
		{
			(new ComparableString("TST-1") < "TST-2").Should().BeTrue();
		}

		[Fact]
		public void StringLessThanOrEqualsOperator()
		{
			(new ComparableString("TST-1") <= "TST-2").Should().BeTrue();
		}
	}

}

