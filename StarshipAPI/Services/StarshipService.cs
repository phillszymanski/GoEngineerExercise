using Microsoft.EntityFrameworkCore;
using StarshipAPI.Models;

public class StarshipService : IStarshipService
{
    private readonly StarshipDbContext _context;

    public StarshipService(StarshipDbContext context)
    {
        _context = context;
    }

    public async Task AddStarshipAsync(Starship starship)
    {
        await _context.Starships.AddAsync(starship);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteStarshipAsync(int id)
    {
        await _context.Starships.Where(s => s.Id == id).ExecuteDeleteAsync();
    }

    public async Task<List<Starship>> GetAllStarshipsAsync()
    {
        return await _context.Starships.ToListAsync();
    }

    public async Task<Starship?> GetStarshipByIdAsync(int id)
    {
        return await _context.Starships.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task UpdateStarshipAsync(Starship starship)
    {
        var existingStarship = await _context.Starships.FindAsync(starship.Id);
        if(existingStarship == null)
        {
            throw new KeyNotFoundException($"Starship with ID {starship.Id} not found.");
        }

        existingStarship.Name = starship.Name;
        existingStarship.Model = starship.Model;
        existingStarship.Manufacturer = starship.Manufacturer;
        existingStarship.CostInCredits = starship.CostInCredits;
        existingStarship.Length = starship.Length;
        existingStarship.MaxAtmospheringSpeed = starship.MaxAtmospheringSpeed;
        existingStarship.Crew = starship.Crew;
        existingStarship.Passengers = starship.Passengers;
        existingStarship.CargoCapacity = starship.CargoCapacity;
        existingStarship.Consumables = starship.Consumables;
        existingStarship.HyperdriveRating = starship.HyperdriveRating;
        existingStarship.MGLT = starship.MGLT;
        existingStarship.StarshipClass = starship.StarshipClass;

        _context.Starships.Update(existingStarship);
        await _context.SaveChangesAsync();
    }
}