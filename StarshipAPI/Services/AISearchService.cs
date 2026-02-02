using System.Text;
using System.Text.Json;
using StarshipAPI.Models;

namespace StarshipAPI.Services
{
    public class AiSearchService : IAiSearchService
    {
        public readonly IConfiguration _configuration;
        public readonly HttpClient _httpClient;
        public readonly ILogger<AiSearchService> _logger;

        public AiSearchService(IConfiguration configuration, HttpClient httpClient, ILogger<AiSearchService> logger)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<SearchResult> SearchAsync(string query, List<Starship> allStarships)
        {
            var apiKey = _configuration["Groq:ApiKey"];

            if(string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("OpenAI API key not configured. Falling back to basic search.");
                return new SearchResult
                {
                    Results = FallbackSearch(query, allStarships),
                    UsedFallback = true,
                    FallbackReason = "API key not configured. Go to https://console.groq.com to generate an API key to use AI Search."
                };
            }

            try
            {
                var starshipsJson = JsonSerializer.Serialize(allStarships.Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Model,
                    s.Manufacturer,
                    s.CostInCredits,
                    s.MaxAtmospheringSpeed,
                    s.StarshipClass,
                    s.Length,
                    s.CargoCapacity,
                    s.Crew
                }));

                var userPrompt = $@"You are a starship database assisstant. Given the following user query and list of starships, return the IDs of matching starships as a JSON array.
                
                User Query: ""{query}""
                Available Starships: {starshipsJson}

                Return ONLY a JSON array of IDs. Examples:
                [1, 5, 9]
                []
                [2]

                If no matches found, return empty array [].";

                var systemPrompt = "You are a helpful assistant that returns only valid JSON arrays of numbers.";
                
                var requestBody = new
                {
                    model = "llama-3.3-70b-versatile",
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    },
                    temperature = 0.0,
                    max_tokens = 200,
                    response_format = new { type = "json_object" }
                };

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var response = await _httpClient.PostAsync(
                    "https://api.groq.com/openai/v1/chat/completions",
                    new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
                );

                if(!response.IsSuccessStatusCode)
                {
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    var errorString = $"Groq API error: {response.StatusCode}\nError details: {errorDetails}";
                    _logger.LogWarning(errorString);
                    return new SearchResult
                    {
                        Results = FallbackSearch(query, allStarships),
                        UsedFallback = true,
                        FallbackReason = GetUserFriendlyErrorMessage(errorDetails)
                    };
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<GroqResponse>(responseContent);
                string aiResponse = result?.Choices[0]?.Message?.Content ?? "[]";

                var matchingIds = ExtractJsonArray(aiResponse);
                return new SearchResult
                {
                    Results = allStarships.Where(s => matchingIds != null && matchingIds.Contains(s.Id)).ToList(),
                    UsedFallback = false
                };
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error during AI search:");
                return new SearchResult
                    {
                        Results = FallbackSearch(query, allStarships),
                        UsedFallback = true,
                        FallbackReason = ex.Message
                    };
            }
        }

        private List<Starship> FallbackSearch(string query, List<Starship> allStarships)
        {
            var queryLowerCase = query.ToLower();
            return allStarships.Where(s => 
                s.Name.ToLower().Contains(queryLowerCase) ||
                s.Model.ToLower().Contains(queryLowerCase) ||
                s.Manufacturer.ToLower().Contains(queryLowerCase) ||
                s.StarshipClass.ToLower().Contains(queryLowerCase)
            ).ToList();
        }

        private List<int> ExtractJsonArray(string resp)
        {
            var startIndex = resp.IndexOf('[');
            var endIndex = resp.LastIndexOf(']');

            if(startIndex >= 0 && endIndex > startIndex)
            {
                var jsonArray = resp.Substring(startIndex, endIndex - startIndex + 1);
                try
                {
                    return JsonSerializer.Deserialize<List<int>>(jsonArray) ?? new List<int>();
                }
                catch(Exception ex)
                {
                    _logger.LogWarning($"Error extracting the JSON array: {ex}");
                    return new List<int>();
                }
            }
            return new List<int>();
        }

        private string GetUserFriendlyErrorMessage(string errorDetails)
        {
            try
            {
                var errorResponse = JsonSerializer.Deserialize<GroqErrorResponse>(errorDetails);
                if (errorResponse?.Error != null)
                {
                    return errorResponse.Error.Code switch
                    {
                        "invalid_api_key" => "Invalid API key",
                        "rate_limit_exceeded" => "Rate limit exceeded",
                        "insufficient_quota" => "API quota exceeded",
                        _ => errorResponse.Error.Message ?? "API error"
                    };
                }
            } catch
            {
                _logger.LogWarning($"Could not parse error response: {errorDetails}");
            }
            return "Unkown API error.";
        }
    }
}