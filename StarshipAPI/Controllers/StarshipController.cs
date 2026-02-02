using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarshipAPI.Models;
using StarshipAPI.Services;

namespace StarshipAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StarshipController : ControllerBase
    {
        private readonly IStarshipService _starshipService;
        private readonly ILogger<StarshipController> _logger;
        private readonly IAiSearchService _aiSearchService;

        public StarshipController(
            IStarshipService starshipService,
            ILogger<StarshipController> logger,
            IAiSearchService aiSearchService)
        {
            _starshipService = starshipService;
            _logger = logger;
            _aiSearchService = aiSearchService;
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

        [HttpPost("search")]
        public async Task<ActionResult<SearchResult>> SearchStarships([FromBody] SearchRequest request) 
        {
            var allStarships = await _starshipService.GetAllStarshipsAsync();
            var results = await _aiSearchService.SearchAsync(request.Query, allStarships);
            return Ok(results);
        }
    }
}