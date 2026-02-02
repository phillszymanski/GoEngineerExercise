using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StarshipAPI.Models;

public class Starship
{
    [JsonPropertyName("name")]
    [StringLength(100, MinimumLength = 1)]
    public required string Name { get; set; }
    
    [JsonPropertyName("model")]
    public required string Model { get; set; }
    
    [JsonPropertyName("manufacturer")]
    public required string Manufacturer { get; set; }
    
    [JsonPropertyName("cost_in_credits")]
    public string CostInCredits { get; set; } = string.Empty;
    
    [JsonPropertyName("length")]
    public string Length { get; set; } = string.Empty;
    
    [JsonPropertyName("max_atmosphering_speed")]
    public string? MaxAtmospheringSpeed { get; set; }
    
    [JsonPropertyName("crew")]
    public string Crew { get; set; } = string.Empty;
    
    [JsonPropertyName("passengers")]
    public string Passengers { get; set; } = string.Empty;
    
    [JsonPropertyName("cargo_capacity")]
    public string? CargoCapacity { get; set; }
    
    [JsonPropertyName("consumables")]
    public string Consumables { get; set; } = string.Empty;
    
    [JsonPropertyName("hyperdrive_rating")]
    public string? HyperdriveRating { get; set; }
    
    [JsonPropertyName("MGLT")]
    public string? MGLT { get; set; }
    
    [JsonPropertyName("starship_class")]
    public string StarshipClass { get; set; } = string.Empty;
    
    [JsonPropertyName("pilots")]
    public List<string> Pilots { get; set; } = new List<string>();
    
    [JsonPropertyName("films")]
    public List<string> Films { get; set; } = new List<string>();
    
    [JsonPropertyName("created")]
    public DateTime? Created { get; set; }
    
    [JsonPropertyName("edited")]
    public DateTime? Edited { get; set; }
    
    [JsonPropertyName("url")]
    public string? Url { get; set; }
    public int Id { get; set; }
}

public class StarshipApiResponse
{
    [JsonPropertyName("count")]
    public int Count { get; set; }
    
    [JsonPropertyName("next")]
    public string Next { get; set; } = string.Empty;
    
    [JsonPropertyName("previous")]
    public string Previous { get; set; } = string.Empty;
    
    [JsonPropertyName("results")]
    public List<Starship> Results { get; set; } = new List<Starship>();
}