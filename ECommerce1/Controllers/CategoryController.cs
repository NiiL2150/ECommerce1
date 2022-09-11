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
    public class CategoryController : ControllerBase
    {
        private readonly ResourceDbContext resourceDbContext;
        private readonly IConfiguration configuration;

        public CategoryController(ResourceDbContext resourceDbContext, IConfiguration configuration)
        {
            this.resourceDbContext = resourceDbContext;
            this.configuration = configuration;
        }

        [HttpPost("add/main")]
        [Authorize(Roles = "Admin")]
        //TODO: Add support for category images
        public async Task<IActionResult> AddMainCategory(Category category)
        {
            if(category.ParentCategory != null)
            {
                return RedirectToAction("AddSubCategory", "Category", new { category });
            }
            Category? foundCategory = await resourceDbContext.Categories.FirstOrDefaultAsync(c => c.Name.ToLower().Trim() == category.Name.ToLower().Trim());
            if(foundCategory != null)
            {
                return BadRequest("Category with such name already exists");
            }
            await resourceDbContext.Categories.AddAsync(category);
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("add/sub")]
        [Authorize(Roles = "Admin")]
        //TODO: Add support for category images
        public async Task<IActionResult> AddSubCategory(Category category)
        {
            if (category.ParentCategory == null)
            {
                return RedirectToAction("AddMainCategory", "Category", new { category });
            }
            if(await resourceDbContext.Categories.FirstOrDefaultAsync(c => c.Id.ToString() == category.ParentCategory.Id.ToString())  == null)
            {
                return BadRequest("No parent category with such id was found");
            }
            Category? foundCategory = await resourceDbContext.Categories.FirstOrDefaultAsync(c => c.Name.ToLower().Trim() == category.Name.ToLower().Trim());
            if (foundCategory != null)
            {
                return BadRequest("Category with such name already exists");
            }
            await resourceDbContext.Categories.AddAsync(category);
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public async Task<IEnumerable<Category>> GetMainCategories()
        {
            List<Category> categories = await resourceDbContext.Categories.Where(c => c.ParentCategory == null).ToListAsync();
            return categories;
        }

        [HttpGet("{guid}")]
        public async Task<IActionResult> GetCategory(string guid)
        {
            Category? category = await resourceDbContext.Categories
                .FirstOrDefaultAsync(c => c.Id.ToString() == guid);
            if (category == null)
            {
                return NotFound("No such category exists");
            }
            if (category.AllowProducts)
            {
                return RedirectToAction("ByCategoryId", "Product", new { guid });
            }
            return RedirectToAction("GetSubCategories", "Category", new { guid });
        }

        [HttpGet("sub/{guid}")]
        public async Task<ActionResult<Category>> GetSubCategories(string guid)
        {
            Category? category = await resourceDbContext.Categories
                .Include(c => c.ChildCategories)
                .FirstOrDefaultAsync(c => c.Id.ToString() == guid);
            if(category == null)
            {
                return NotFound("No such category exists");
            }
            return Ok(category);
        }

        //TODO: Category deletion and edit
    }
}
