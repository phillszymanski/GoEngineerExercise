namespace StarshipAPI.Tests;
using Microsoft.EntityFrameworkCore;
using StarshipAPI.Models;

public class StarshipDbContextTests
{
    private DbContextOptions<StarshipDbContext> _opts;
    public StarshipDbContextTests()
    {
        _opts = new DbContextOptionsBuilder<StarshipDbContext>()
                        .UseInMemoryDatabase(databaseName: "TestDatabase")
                        .Options;
    }

    private Starship CreateTestStarship()
    {
        return new Starship
        {
            Name = "Test Starship",
            Model = "Test Model",
            Manufacturer = "Test Manufacturer",
            CostInCredits = "1000000",
            Length = "100",
            Crew = "10",
            Passengers = "20",
            StarshipClass = "Test Class",
            Consumables = "1 month"
        };
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldSetCreatedAndEdited_WhenEntityAdded()
    {
        using(var context = new StarshipDbContext(_opts)) {
            // Arrange
            var starship = CreateTestStarship();

            // Act
            context.Starships.Add(starship);
            await context.SaveChangesAsync();

            // Assert
            Assert.NotEqual(default(DateTime), starship.Created);
            Assert.NotEqual(default(DateTime), starship.Edited);
            Assert.Equal(starship.Created, starship.Edited);
        }
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldSetEdited_WhenEntityEdited()
    {
        using(var context = new StarshipDbContext(_opts)) {
            // Arrange
            var starship = CreateTestStarship();
            context.Starships.Add(starship);
            await context.SaveChangesAsync();

            var originalCreated = starship.Created;
            var originalEdited = starship.Edited;
            await Task.Delay(10);

            // Act
            starship.Name = "changed";
            await context.SaveChangesAsync();

            // Assert
            Assert.Equal(originalCreated, starship.Created);
            Assert.NotEqual(originalEdited, starship.Edited);
            Assert.True(starship.Edited > originalEdited);
        }
    }
}
