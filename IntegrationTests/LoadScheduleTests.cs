using Microsoft.AspNetCore.Mvc.Testing;
using SbtMultiDB;

namespace IntegrationTests;

public class LoadScheduleTests : IClassFixture<WebApplicationFactory<Program>>
{
    private WebApplicationFactory<Program> Factory {  get; init; }

    public LoadScheduleTests(WebApplicationFactory<Program> factory)
    {
        this.Factory = factory;        
    }


    [Theory]
    [InlineData("/")]
    [InlineData("/Index")]
    [InlineData("/About")]
    [InlineData("/Privacy")]
    [InlineData("/Contact")]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
    {
        // Arrange
        var client = this.Factory.CreateClient();

        // Act
        var response = await client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        Assert.Equal("text/html; charset=utf-8",
            response.Content.Headers.ContentType.ToString());
    }

    [Fact]
    public void LoadScheduleTest()
    {
    }
}