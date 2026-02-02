using System.Text.Json.Serialization;
using StarshipAPI.Models;

public class GroqResponse
{
    [JsonPropertyName("choices")]
    public List<Choice> Choices { get; set; } = new();
}

public class Choice
{
    [JsonPropertyName("message")]
    public Message Message { get; set; } = new();
}

public class Message
{
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

public class SearchRequest
{
    public required string Query { get; set; }
}

public class SearchResult
{
    public List<Starship> Results { get; set; } = new();
    public bool UsedFallback { get; set; }
    public string? FallbackReason { get; set; }
}

public class GroqErrorResponse
{
    [JsonPropertyName("error")]
    public GroqError? Error { get; set; }
}

public class GroqError
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    [JsonPropertyName("code")]
    public string? Code { get; set; }
}