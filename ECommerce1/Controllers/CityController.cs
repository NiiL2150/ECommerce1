using ECommerce1.Models;
using ECommerce1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CityController : ControllerBase
    {
        private readonly ResourceDbContext resourceDbContext;

        public CityController(ResourceDbContext resourceDbContext, IConfiguration configuration)
        {
            this.resourceDbContext = resourceDbContext;
        }

        [HttpGet("get")]
        public async Task<IEnumerable<City>> GetAsync()
        {
            return await resourceDbContext.Cities.ToListAsync();
        }

        [HttpPost("add")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Bad name");
            }
            if(resourceDbContext.Cities.FirstOrDefault(c => c.Name.ToLower().Trim() == name.ToLower().Trim()) != null)
            {
                return BadRequest("City already exists");
            }
            City city = new() { Name = name };
            await resourceDbContext.Cities.AddAsync(city);
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAsync(string name)
        {
            City? city = resourceDbContext.Cities.FirstOrDefault(c => c.Name.ToLower().Trim() == name.ToLower().Trim());
            if(city == null)
            {
                return BadRequest("No such city");
            }
            resourceDbContext.Cities.Remove(city);
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
