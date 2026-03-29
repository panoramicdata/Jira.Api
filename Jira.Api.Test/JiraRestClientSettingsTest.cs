namespace Jira.Api.Test;

public class JiraRestClientSettingsTest
{
    public class UserAgentConstructor
    {
        [Theory]
        [InlineData("MyApp")]
        [InlineData("MyApp/1.0")]
        [InlineData("MyApp/1.0.0")]
        [InlineData("MyApp/1.0-beta")]
        [InlineData("MyApp/1.0 AnotherProduct/2.0")]
        [InlineData("My_App/1.0")]
        [InlineData("My-App/1.0")]
        [InlineData("My.App/1.0")]
        [InlineData("My~App/1.0")]
        [InlineData("A")]
        public void ShouldAcceptValidUserAgent(string userAgent)
        {
            var settings = new JiraRestClientSettings(userAgent);

            settings.UserAgent.Should().Be(userAgent);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void ShouldThrowArgumentExceptionForWhitespaceUserAgent(string userAgent)
        {
            var act = () => new JiraRestClientSettings(userAgent);
            act.Should().ThrowExactly<ArgumentException>();
        }

        [Fact]
        public void ShouldThrowArgumentNullExceptionForNullUserAgent()
        {
            var act = () => new JiraRestClientSettings(null!);
            act.Should().ThrowExactly<ArgumentNullException>();
        }

        [Theory]
        [InlineData("/1.0")]
        [InlineData("My/App/Extra")]
        [InlineData(".MyApp")]
        [InlineData("-MyApp")]
        [InlineData("MyApp/")]
        [InlineData("MyApp/ 1.0")]
        [InlineData("My\tApp")]
        [InlineData("My@App")]
        [InlineData("My#App")]
        public void ShouldThrowFormatExceptionForInvalidUserAgent(string userAgent)
        {
            var act = () => new JiraRestClientSettings(userAgent);
            var ex = act.Should().ThrowExactly<FormatException>().Which;

            ex.Message.Should().Contain("not a valid RFC 9110 product token");
            ex.Message.Should().Contain(userAgent);
        }
    }

    public class DefaultConstructor
    {
#pragma warning disable CS0618 // Testing the obsolete parameterless constructor
        [Fact]
        public void ShouldUseDefaultUserAgent()
        {
            var settings = new JiraRestClientSettings();

            settings.UserAgent.Should().Be(JiraRestClientSettings.DefaultUserAgent);
        }

        [Fact]
        public void ShouldInitializeJsonSerializerSettings()
        {
            var settings = new JiraRestClientSettings();

            settings.JsonSerializerSettings.Should().NotBeNull();
            settings.JsonSerializerSettings.NullValueHandling.Should().Be(Newtonsoft.Json.NullValueHandling.Ignore);
        }

        [Fact]
        public void ShouldInitializeCache()
        {
            var settings = new JiraRestClientSettings();

            settings.Cache.Should().NotBeNull();
        }

        [Fact]
        public void ShouldInitializeCustomFieldSerializers()
        {
            var settings = new JiraRestClientSettings();

            settings.CustomFieldSerializers.Should().NotBeEmpty();
        }
#pragma warning restore CS0618
    }

    public class UserAgentProperty
    {
        [Fact]
        public void ShouldReturnExplicitlyProvidedUserAgent()
        {
            var settings = new JiraRestClientSettings("MyCustomApp/2.5");

            settings.UserAgent.Should().Be("MyCustomApp/2.5");
        }

        [Fact]
        public void DefaultUserAgentConstShouldBeValid()
        {
            // Verify the default user agent constant is itself a valid format
            var settings = new JiraRestClientSettings(JiraRestClientSettings.DefaultUserAgent);

            settings.UserAgent.Should().Be(JiraRestClientSettings.DefaultUserAgent);
        }
    }
}
