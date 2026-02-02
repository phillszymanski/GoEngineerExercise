using StarshipAPI.Models;

namespace StarshipAPI.Services
{
    public interface IAiSearchService
    {
        Task<SearchResult> SearchAsync(string query, List<Starship> allStarships);
    }
}