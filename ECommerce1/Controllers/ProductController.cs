using ECommerce1.Models;
using ECommerce1.Models.ViewModels;
using ECommerce1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static ECommerce1.Models.Product;
using static ECommerce1.Models.ViewModels.ProductsViewModel;

namespace ECommerce1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ResourceDbContext resourceDbContext;
        private readonly IConfiguration configuration;

        public ProductController(ResourceDbContext resourceDbContext, IConfiguration configuration)
        {
            this.resourceDbContext = resourceDbContext;
            this.configuration = configuration;
        }

        [HttpGet("{guid}")]
        public async Task<ActionResult<Product>> GetProduct(string guid)
        {
            Product? product = await resourceDbContext.Products
                .Include(p => p.Category).Include(p => p.User)
                .Include(p => p.ProductPhotos)
                .FirstOrDefaultAsync(p => p.Id.ToString() == guid);

            if(product == null)
            {
                return NotFound("No such product exists");
            }
            return Ok(product);
        }


        [HttpGet("category/{guid}")]
        public async Task<ActionResult<ProductsViewModel>> ByCategoryId(string guid, int page = 1, int onPage = 20, ProductSorting sorting = ProductSorting.NewerFirst)
        {
            Category? category = await resourceDbContext.Categories
                .FirstOrDefaultAsync(c => c.Id.ToString() == guid);
            if (category == null)
            {
                return NotFound("No such category exists");
            }

            IQueryable<Product> unorderedProducts = resourceDbContext.Products.Where(p => p.Category.Id.ToString() == guid);

            int totalCount = unorderedProducts.Count();
            int totalPages = (int)Math.Ceiling((double)totalCount / onPage);
            if (page > totalPages)
            {
                page = totalPages;
            }

            IOrderedQueryable<Product> orderedProducts;

            switch (sorting)
            {
                case ProductSorting.OlderFirst:
                    orderedProducts = unorderedProducts.OrderBy(p => p.CreationTime);
                    break;
                case ProductSorting.NewerFirst:
                    orderedProducts = unorderedProducts.OrderByDescending(p => p.CreationTime);
                    break;
                case ProductSorting.CheaperFirst:
                    orderedProducts = unorderedProducts.OrderBy(p => p.Price);
                    break;
                case ProductSorting.ExpensiveFirst:
                    orderedProducts = unorderedProducts.OrderByDescending(p => p.Price);
                    break;
                default:
                    return NotFound("Sorting error occurred!");
            }

            List<Product> products = await orderedProducts.Skip((page-1)*onPage).Take(onPage).ToListAsync();

            ProductsViewModel viewModel = new()
            {
                Products = products,
                Category = category,
                TotalProductCount = totalCount,
                TotalPageCount = totalPages,
                OnPageProductCount = onPage,
                CurrentPage = page
            };

            return Ok(viewModel);
        }

        [HttpPost("add/{categoryId}")]
        [Authorize]
        //TODO: Add photo support
        public async Task<IActionResult> AddMainCategory(Product product, string categoryId)
        {
            string? username = HttpContext.User.Identity.Name;
            Profile? user = await resourceDbContext.Profiles.FirstOrDefaultAsync(p => p.Username == username);
            if (user == null)
            {
                return BadRequest();
            }
            Category? category = await resourceDbContext.Categories.FirstOrDefaultAsync(c => c.Id.ToString() == categoryId);
            if(category == null)
            {
                return BadRequest("No such category exists");
            }
            Product prod = new()
            {
                Name = product.Name,
                CreationTime = DateTime.UtcNow,
                Description = product.Description,
                Price = product.Price,
                Category = category,
                User = user
            };
            await resourceDbContext.Products.AddAsync(prod);
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }

        //TODO: Product edit for admin

        [HttpDelete("delete/{guid}")]
        //TODO: Product deletion for product creator
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAsync(string guid)
        {
            Product? product = await resourceDbContext.Products.FirstOrDefaultAsync(p => p.Id.ToString() == guid);
            if(product == null)
            {
                return BadRequest("No product with such id exists");
            }
            resourceDbContext.Products.Remove(product);
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
