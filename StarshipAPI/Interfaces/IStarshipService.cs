using StarshipAPI.Models;

public interface IStarshipService
{
    Task<List<Starship>> GetAllStarshipsAsync();
    Task<Starship?> GetStarshipByIdAsync(int id);
    Task AddStarshipAsync(Starship starship);
    Task UpdateStarshipAsync(Starship starship);
    Task DeleteStarshipAsync(int id);
}