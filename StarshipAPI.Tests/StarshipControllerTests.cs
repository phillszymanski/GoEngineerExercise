using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using StarshipAPI.Controllers;
using StarshipAPI.Models;
using StarshipAPI.Services;
using Xunit;

namespace StarshipAPI.Tests
{
    public class StarshipControllerTests
    {
        private readonly Mock<IStarshipService> _mockStarshipService;
        private readonly Mock<ILogger<StarshipController>> _mockLogger;
        private readonly Mock<IAiSearchService> _aiSearchService;
        private readonly StarshipController _controller;

        public StarshipControllerTests()
        {
            _mockStarshipService = new Mock<IStarshipService>();
            _mockLogger = new Mock<ILogger<StarshipController>>();
            _aiSearchService = new Mock<IAiSearchService>();
            
            _controller = new StarshipController(
                _mockStarshipService.Object,
                _mockLogger.Object,
                _aiSearchService.Object
            );
        }

        private Starship CreateTestStarship(int id = 1, string name = "Test Starship")
        {
            return new Starship
            {
                Id = id,
                Name = name,
                Model = "Test Model",
                Manufacturer = "Incom Corporation",
                CostInCredits = "149999",
                Length = "12.5",
                MaxAtmospheringSpeed = "1050",
                Crew = "1",
                Passengers = "0",
                CargoCapacity = "110",
                Consumables = "1 week",
                HyperdriveRating = "1.0",
                MGLT = "100",
                StarshipClass = "Starfighter"
            };
        }

        [Fact]
        public async Task GetAll_ShouldReturnOkWithStarships()
        {
            // Arrange
            var starships = new List<Starship>
            {
                CreateTestStarship(1, "Test Starship"),
                CreateTestStarship(2, "Test Starship 2")
            };

            _mockStarshipService
                .Setup(s => s.GetAllStarshipsAsync())
                .ReturnsAsync(starships);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedStarships = Assert.IsType<List<Starship>>(okResult.Value);
            Assert.Equal(2, returnedStarships.Count);
            _mockStarshipService.Verify(s => s.GetAllStarshipsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAll_ShouldReturnEmptyList_WhenNoStarships()
        {
            // Arrange
            _mockStarshipService
                .Setup(s => s.GetAllStarshipsAsync())
                .ReturnsAsync(new List<Starship>());

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedStarships = Assert.IsType<List<Starship>>(okResult.Value);
            Assert.Empty(returnedStarships);
        }

        [Fact]
        public async Task AddStarship_ShouldReturnOkWithStarship()
        {
            // Arrange
            var starship = CreateTestStarship(0, "Test Starship");
            
            _mockStarshipService
                .Setup(s => s.AddStarshipAsync(It.IsAny<Starship>()))
                .Returns(Task.CompletedTask)
                .Callback<Starship>(s => s.Id = 42); // Simulate ID assignment

            // Act
            var result = await _controller.AddStarship(starship);

            // Assert
            var okObjectResult = Assert.IsType<OkObjectResult>(result);
            var returnedStarship = Assert.IsType<Starship>(okObjectResult.Value);
            Assert.Equal("Test Starship", returnedStarship.Name);
            Assert.Equal(42, returnedStarship.Id);
            
            _mockStarshipService.Verify(s => s.AddStarshipAsync(starship), Times.Once);
        }

        [Fact]
        public async Task AddStarship_ShouldCallServiceWithCorrectData()
        {
            // Arrange
            var starship = CreateTestStarship(0, "Test Starship");
            Starship? capturedStarship = null;

            _mockStarshipService
                .Setup(s => s.AddStarshipAsync(It.IsAny<Starship>()))
                .Returns(Task.CompletedTask)
                .Callback<Starship>(s => capturedStarship = s);

            // Act
            await _controller.AddStarship(starship);

            // Assert
            Assert.NotNull(capturedStarship);
            Assert.Equal("Test Starship", capturedStarship.Name);
            Assert.Equal("Test Model", capturedStarship.Model);
        }

        [Fact]
        public async Task UpdateStarship_ShouldReturnNoContent()
        {
            // Arrange
            var starship = CreateTestStarship(1, "Updated Test Starship");
            
            _mockStarshipService
                .Setup(s => s.UpdateStarshipAsync(It.IsAny<Starship>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateStarship(starship);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockStarshipService.Verify(s => s.UpdateStarshipAsync(starship), Times.Once);
        }

        [Fact]
        public async Task UpdateStarship_ShouldPassCorrectDataToService()
        {
            // Arrange
            var starship = CreateTestStarship(5, "Modified Ship");
            starship.Crew = "10";
            starship.Passengers = "20";
            
            Starship? capturedStarship = null;
            _mockStarshipService
                .Setup(s => s.UpdateStarshipAsync(It.IsAny<Starship>()))
                .Returns(Task.CompletedTask)
                .Callback<Starship>(s => capturedStarship = s);

            // Act
            await _controller.UpdateStarship(starship);

            // Assert
            Assert.NotNull(capturedStarship);
            Assert.Equal(5, capturedStarship.Id);
            Assert.Equal("Modified Ship", capturedStarship.Name);
            Assert.Equal("10", capturedStarship.Crew);
            Assert.Equal("20", capturedStarship.Passengers);
        }

        [Fact]
        public async Task DeleteStarship_ShouldReturnNoContent()
        {
            // Arrange
            var starshipId = 1;
            
            _mockStarshipService
                .Setup(s => s.DeleteStarshipAsync(starshipId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteStarship(starshipId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockStarshipService.Verify(s => s.DeleteStarshipAsync(starshipId), Times.Once);
        }

        [Fact]
        public async Task DeleteStarship_ShouldCallServiceWithCorrectId()
        {
            // Arrange
            var starshipId = 42;
            int capturedId = 0;
            
            _mockStarshipService
                .Setup(s => s.DeleteStarshipAsync(It.IsAny<int>()))
                .Returns(Task.CompletedTask)
                .Callback<int>(id => capturedId = id);

            // Act
            await _controller.DeleteStarship(starshipId);

            // Assert
            Assert.Equal(42, capturedId);
        }
    }
}
