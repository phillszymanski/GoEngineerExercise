using System.Data.Common;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using StarshipAPI.Models;
using StarshipAPI.Services;

namespace StarshipAPI.Tests
{
    public class AiSearchServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<AiSearchService>> _mockLogger;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly AiSearchService _service;
        private readonly List<Starship> _testStarships;

        public AiSearchServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<AiSearchService>>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);

            _service = new AiSearchService(_mockConfiguration.Object, _httpClient, _mockLogger.Object);

            _testStarships = new List<Starship>
            {
                new Starship
                {
                    Id = 1,
                    Name = "Test Ship",
                    Model = "Test Model",
                    Manufacturer = "Test Manufacturer",
                    CostInCredits = "10000",
                    MaxAtmospheringSpeed = "1000",
                    StarshipClass = "Test Class",
                },
                new Starship
                {
                    Id = 2,
                    Name = "Test Ship 2",
                    Model = "Test Model 2",
                    Manufacturer = "Test Manufacturer 2",
                    CostInCredits = "20000",
                    MaxAtmospheringSpeed = "2000",
                    StarshipClass = "Test Class 2",
                },
            };
        }

        [Fact]
        public async Task SearchAsync_WithValidApiKey_ReturnsMatchingStarships()
        {
            var query = "Test ship";
            _mockConfiguration.Setup(x => x["Groq:ApiKey"]).Returns("test-api-key");

            var groqResponse = new
            {
                id = "test-id",
                @object = "chat.completion",
                created = 2134567890,
                model = "test-model",
                choices = new[]
                {
                    new
                    {
                        index = 0,
                        message = new
                        {
                            role = "assistant",
                            content = "[1]"
                        }
                    }
                }
            };

            var responseContent = new StringContent(JsonSerializer.Serialize(groqResponse));
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = responseContent
            };

            _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

            var result = await _service.SearchAsync(query, _testStarships);

            Assert.NotNull(result);
            Assert.Single(result.Results);
            Assert.Equal(1, result.Results[0].Id);
            Assert.Equal("Test Ship", result.Results[0].Name);
        }

        [Fact]
        public async Task SearchAsync_WithNoApiKey_UsesFallbackSearch()
        {
            var query = "Test ship 2";
            _mockConfiguration.Setup(x => x["Groq:ApiKey"]).Returns((string?)null);

            var result = await _service.SearchAsync(query, _testStarships);

            Assert.NotNull(result);
            Assert.Single(result.Results);
            Assert.Equal(2, result.Results[0].Id);
            Assert.Equal("Test Ship 2", result.Results[0].Name);

            _mockHttpMessageHandler.Protected()
            .Verify<Task<HttpResponseMessage>>(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task SearchAsync_WhenApiReturnsError_FallsBackToBasicSearch()
        {
            var query = "Test ship 2";
            _mockConfiguration.Setup(x => x["Groq:ApiKey"]).Returns("test-api-key");

            var groqResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("API Error")
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(groqResponse);

            var result = await _service.SearchAsync(query, _testStarships);

            Assert.NotNull(result);
            Assert.Single(result.Results);
            Assert.Equal("Test Ship 2", result.Results[0].Name);
        }

        [Fact]
        public async Task SearchAsync_WithMarkdownWrappedJson_ExtractsArrayCorrectly()
        {
            var query = "Test ship 2";
            _mockConfiguration.Setup(x => x["Groq:ApiKey"]).Returns("test-api-key");

            var groqResponse = new
            {
                id = "test-id",
                @object = "chat.completion",
                created = 2134567890,
                model = "test-model",
                choices = new[]
                {
                    new
                    {
                        index = 0,
                        message = new
                        {
                            role = "assistant",
                            content = "```json\n[2]\n```"
                        }
                    }
                }
            };

            var responseContent = new StringContent(JsonSerializer.Serialize(groqResponse));
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = responseContent
            };

            _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

            var result = await _service.SearchAsync(query, _testStarships);

            Assert.NotNull(result);
            Assert.Single(result.Results);
            Assert.Equal(2, result.Results[0].Id);
        }

        [Fact]
        public async Task SearchAsync_WithInvalidJsonArray_FallsBackToBasicSearch()
        {
            var query = "Test ship";
            _mockConfiguration.Setup(x => x["Groq:ApiKey"]).Returns("test-api-key");

            var groqResponse = new
            {
                id = "test-id",
                @object = "chat.completion",
                created = 2134567890,
                model = "test-model",
                choices = new[]
                {
                    new
                    {
                        index = 0,
                        message = new
                        {
                            role = "assistant",
                            content = "[1, invalid]"
                        }
                    }
                }
            };

            var responseContent = new StringContent(JsonSerializer.Serialize(groqResponse));
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = responseContent
            };

            _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

            var result = await _service.SearchAsync(query, _testStarships);

            Assert.NotNull(result);
            Assert.Empty(result.Results);
        }

        [Fact]
        public async Task SearchAsync_WithEmptyStringJsonArray_FallsBackToBasicSearch()
        {
            var query = "Test ship";
            _mockConfiguration.Setup(x => x["Groq:ApiKey"]).Returns("test-api-key");

            var groqResponse = new
            {
                id = "test-id",
                @object = "chat.completion",
                created = 2134567890,
                model = "test-model",
                choices = new[]
                {
                    new
                    {
                        index = 0,
                        message = new
                        {
                            role = "assistant",
                            content = ""
                        }
                    }
                }
            };

            var responseContent = new StringContent(JsonSerializer.Serialize(groqResponse));
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = responseContent
            };

            _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

            var result = await _service.SearchAsync(query, _testStarships);

            Assert.NotNull(result.Results);
            Assert.Empty(result.Results);
        }

        [Theory]
        [InlineData("invalid_api_key", "Invalid API Key provided", "Invalid API key")]
        [InlineData("rate_limit_exceeded", "Rate limit exceeded", "Rate limit exceeded")]
        [InlineData("insufficient_quota", "Insufficient quota", "API quota exceeded")]
        [InlineData("unknown_error", "Something went wrong", "Something went wrong")]
        public async Task GetUserFriendlyErrorMessage_WithValidErrorResponse_ReturnsUserFriendlyMessage(
            string errorCode, string errorMessage, string expectedFallbackReason)
        {
            var query = "Test ship";
            _mockConfiguration.Setup(x => x["Groq:ApiKey"]).Returns("test-api-key");

            var errorResponse = new
            {
                error = new
                {
                    message = errorMessage,
                    type = "invalid_request_error",
                    code = errorCode
                }
            };

            var responseContent = new StringContent(JsonSerializer.Serialize(errorResponse));
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = responseContent
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            var result = await _service.SearchAsync(query, _testStarships);

            Assert.True(result.UsedFallback);
            Assert.Equal(expectedFallbackReason, result.FallbackReason);
            Assert.Equal(2, result.Results.Count);
        }
    }
}