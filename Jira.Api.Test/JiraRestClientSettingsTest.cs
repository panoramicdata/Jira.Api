using System;
using Xunit;

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

            Assert.Equal(userAgent, settings.UserAgent);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void ShouldThrowArgumentExceptionForWhitespaceUserAgent(string userAgent)
        {
            Assert.Throws<ArgumentException>(() => new JiraRestClientSettings(userAgent));
        }

        [Fact]
        public void ShouldThrowArgumentNullExceptionForNullUserAgent()
        {
            Assert.Throws<ArgumentNullException>(() => new JiraRestClientSettings(null!));
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
            var ex = Assert.Throws<FormatException>(() => new JiraRestClientSettings(userAgent));

            Assert.Contains("not a valid RFC 9110 product token", ex.Message);
            Assert.Contains(userAgent, ex.Message);
        }
    }

    public class DefaultConstructor
    {
#pragma warning disable CS0618 // Testing the obsolete parameterless constructor
        [Fact]
        public void ShouldUseDefaultUserAgent()
        {
            var settings = new JiraRestClientSettings();

            Assert.Equal(JiraRestClientSettings.DefaultUserAgent, settings.UserAgent);
        }

        [Fact]
        public void ShouldInitializeJsonSerializerSettings()
        {
            var settings = new JiraRestClientSettings();

            Assert.NotNull(settings.JsonSerializerSettings);
            Assert.Equal(Newtonsoft.Json.NullValueHandling.Ignore, settings.JsonSerializerSettings.NullValueHandling);
        }

        [Fact]
        public void ShouldInitializeCache()
        {
            var settings = new JiraRestClientSettings();

            Assert.NotNull(settings.Cache);
        }

        [Fact]
        public void ShouldInitializeCustomFieldSerializers()
        {
            var settings = new JiraRestClientSettings();

            Assert.NotEmpty(settings.CustomFieldSerializers);
        }
#pragma warning restore CS0618
    }

    public class UserAgentProperty
    {
        [Fact]
        public void ShouldReturnExplicitlyProvidedUserAgent()
        {
            var settings = new JiraRestClientSettings("MyCustomApp/2.5");

            Assert.Equal("MyCustomApp/2.5", settings.UserAgent);
        }

        [Fact]
        public void DefaultUserAgentConstShouldBeValid()
        {
            // Verify the default user agent constant is itself a valid format
            var settings = new JiraRestClientSettings(JiraRestClientSettings.DefaultUserAgent);

            Assert.Equal(JiraRestClientSettings.DefaultUserAgent, settings.UserAgent);
        }
    }
}
