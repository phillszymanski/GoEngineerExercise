using Microsoft.EntityFrameworkCore;
using StarshipAPI.Models;

public class StarshipSeeder : IStarshipSeeder
{
    private readonly HttpClient _httpClient;
    private readonly IDbContextFactory<StarshipDbContext> _contextFactory;

    public StarshipSeeder(HttpClient httpClient, IDbContextFactory<StarshipDbContext> contextFactory)
    {
        _httpClient = httpClient;
        _contextFactory = contextFactory;
    }

    public async Task SeedStarshipsAsync()
    {
        var starships = new List<Starship>();
        var nextUrl = "https://swapi.dev/api/starships/";

        while (nextUrl != null)
        {
            var response = await _httpClient.GetFromJsonAsync<StarshipApiResponse>(nextUrl);
            if (response != null)
            {
                foreach (var starship in response.Results)
                {
                    starship.Id = ExtractIdFromUrl(starship.Url);
                }
                starships.AddRange(response.Results);
                nextUrl = response.Next;
            }
            else
            {
                nextUrl = null;
            }
        }

        using var context = _contextFactory.CreateDbContext();

        // Use a transaction to ensure IDENTITY_INSERT state is managed properly
        await using var transaction = await context.Database.BeginTransactionAsync();
        
        try
        {
            // Enable IDENTITY_INSERT
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Starships ON");
            
            // Add and save starships
            await context.Starships.AddRangeAsync(starships);
            await context.SaveChangesAsync();
            
            // Disable IDENTITY_INSERT
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Starships OFF");
            
            // Commit transaction
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private int ExtractIdFromUrl(string url)
    {
        var segments = url.TrimEnd('/').Split('/');
        return int.Parse(segments[^1]);
    }
}