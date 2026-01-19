using AwesomeAssertions;

namespace Jira.Api.Test;

public class QueryParametersTest
{
	[Fact]
	public void GetQueryParametersFromPath()
	{
		// Arrange
		var url = "?field1=9&field2=Test";

		// Act
		var parameters = QueryParametersHelper.GetParametersFromPath(url);

		// Assert
		parameters.Should().NotBeNull();
		parameters.Should().HaveCount(2);

		parameters.Should().ContainSingle(p => p.Name == "field1" && p.Value.ToString() == "9");
		parameters.Should().ContainSingle(p => p.Name == "field2" && p.Value.ToString() == "Test");
	}

	[Fact]
	public void GetQueryParametersFromPathNoEqual()
	{
		// Arrange
		var url = "?field1";

		// Act
		var parameters = QueryParametersHelper.GetParametersFromPath(url);

		// Assert
		parameters.Should().NotBeNull();
		parameters.Should().ContainSingle();
		parameters.Should().ContainSingle(p => p.Name == "field1" && p.Value.ToString() == "");
	}

	[Fact]
	public void GetQueryParametersFromPathMultipleEquals()
	{
		// Arrange
		var url = "?field1=value=string==";

		// Act
		var parameters = QueryParametersHelper.GetParametersFromPath(url);

		// Assert
		parameters.Should().NotBeNull();
		parameters.Should().ContainSingle();

		parameters.First().Name.Should().Be("field1");
		parameters.First().Value.ToString().Should().Be("value=string==");
	}
}

