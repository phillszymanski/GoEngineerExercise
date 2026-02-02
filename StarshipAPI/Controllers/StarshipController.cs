using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarshipAPI.Models;

namespace StarshipAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StarshipController : ControllerBase
    {
        private readonly IStarshipService _starshipService;
        private readonly ILogger<StarshipController> _logger;

        public StarshipController(IStarshipService starshipService, ILogger<StarshipController> logger)
        {
            _starshipService = starshipService;
            _logger = logger;
        }

        [HttpGet("GetAllStarships")]
        public async Task<ActionResult<List<Starship>>> GetAll()
        {
            var starships = await _starshipService.GetAllStarshipsAsync();
            return Ok(starships);
        }

        [HttpPost("AddStarship")]
        public async Task<IActionResult> AddStarship([FromBody] Starship starship)
        {
            _logger.LogInformation($"Adding starship: {starship.Name}");
            await _starshipService.AddStarshipAsync(starship);
            return Ok(starship);
        }

        [HttpPut("UpdateStarship")]
        public async Task<IActionResult> UpdateStarship([FromBody] Starship starship)
        {
            await _starshipService.UpdateStarshipAsync(starship);
            return NoContent();
        }

        [HttpDelete("DeleteStarship/{id}")]
        public async Task<IActionResult> DeleteStarship(int id)
        {
            _logger.LogInformation($"Deleting starship with id={id}");
            await _starshipService.DeleteStarshipAsync(id);
            return NoContent();
        }
    }
}