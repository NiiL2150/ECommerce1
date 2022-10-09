using ECommerce1.Models;
using ECommerce1.Models.ViewModels;
using ECommerce1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static ECommerce1.Models.ViewModels.ProductsViewModel;

namespace ECommerce1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ResourceDbContext resourceDbContext;

        public ProductController(ResourceDbContext resourceDbContext)
        {
            this.resourceDbContext = resourceDbContext;
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

        [HttpGet("title/{title}")]
        public async Task<ActionResult<ProductsViewModel>> ByTitle(string title, int page = 1, int onPage = 20, ProductSorting sorting = ProductSorting.NewerFirst)
        {
            IQueryable<ProductsProductViewModel> unorderedProducts = resourceDbContext.Products
                .Where(p => EF.Functions.Like(p.Name, $"%{title}%"))
                .Select(p => new ProductsProductViewModel()
                {
                    Id = p.Id,
                    CreationTime = p.CreationTime,
                    Description = p.Description,
                    FirstPhotoUrl = p.ProductPhotos.Count == 0 ? "" : p.ProductPhotos[0].Url,
                    Name = p.Name,
                    Price = p.Price
                });

            int totalCount = unorderedProducts.Count();
            int totalPages = (int)Math.Ceiling((double)totalCount / onPage);

            IEnumerable<ProductsProductViewModel> products;

            try
            {
                products = await PrepareProducts(unorderedProducts, page, onPage, sorting);
            }
            catch (Exception)
            {
                return NotFound("Sorting error occurred!");
            }

            ProductsViewModelByTitle viewModel = new()
            {
                Products = unorderedProducts,
                Title = title,
                TotalProductCount = totalCount,
                TotalPageCount = totalPages,
                OnPageProductCount = onPage,
                CurrentPage = page
            };

            return Ok(viewModel);
        }

        [HttpGet("user/{guid}")]
        public async Task<ActionResult<ProductsViewModel>> ByUserId(string guid, int page = 1, int onPage = 20, ProductSorting sorting = ProductSorting.NewerFirst)
        {
            Profile? user = await resourceDbContext.Profiles
                .FirstOrDefaultAsync(c => c.Id.ToString() == guid);
            if (user == null)
            {
                return NotFound("No such category exists");
            }

            IQueryable<ProductsProductViewModel> unorderedProducts = resourceDbContext.Products
                .Where(p => p.User.Id.ToString() == guid)
                .Select(p => new ProductsProductViewModel()
                {
                    Id = p.Id,
                    CreationTime = p.CreationTime,
                    Description = p.Description,
                    FirstPhotoUrl = p.ProductPhotos.Count == 0 ? "" : p.ProductPhotos[0].Url,
                    Name = p.Name,
                    Price = p.Price
                });

            int totalCount = unorderedProducts.Count();
            int totalPages = (int)Math.Ceiling((double)totalCount / onPage);

            IEnumerable<ProductsProductViewModel> products;

            try
            {
                products = await PrepareProducts(unorderedProducts, page, onPage, sorting);
            }
            catch (Exception)
            {
                return NotFound("Sorting error occurred!");
            }

            ProductsViewModelByUser viewModel = new()
            {
                Products = unorderedProducts,
                Profile = user,
                TotalProductCount = totalCount,
                TotalPageCount = totalPages,
                OnPageProductCount = onPage,
                CurrentPage = page
            };

            return Ok(viewModel);
        }

        //TODO: Add image support, if uncommented, will break
        [HttpGet("category/{guid}")]
        public async Task<ActionResult<ProductsViewModel>> ByCategoryId(string guid, int page = 1, int onPage = 20, ProductSorting sorting = ProductSorting.NewerFirst)
        {
            Category? category = await resourceDbContext.Categories
                .FirstOrDefaultAsync(c => c.Id.ToString() == guid);
            if (category == null)
            {
                return NotFound("No such category exists");
            }

            IQueryable<ProductsProductViewModel> unorderedProducts = resourceDbContext.Products
                .Where(p => p.Category.Id.ToString() == guid)
                .Select(p => new ProductsProductViewModel()
                {
                    Id = p.Id,
                    CreationTime = p.CreationTime,
                    Description = p.Description,
                    FirstPhotoUrl = p.ProductPhotos.Count == 0 ? "" : p.ProductPhotos[0].Url,
                    Name = p.Name,
                    Price = p.Price
                });

            int totalCount = unorderedProducts.Count();
            int totalPages = (int)Math.Ceiling((double)totalCount / onPage);

            IEnumerable<ProductsProductViewModel> products;

            try
            {
                products = await PrepareProducts(unorderedProducts, page, onPage, sorting);
            }
            catch (Exception)
            {
                return NotFound("Sorting error occurred!");
            }

            ProductsViewModelByCategory viewModel = new()
            {
                Products = unorderedProducts,
                Category = category,
                TotalProductCount = totalCount,
                TotalPageCount = totalPages,
                OnPageProductCount = onPage,
                CurrentPage = page
            };

            return Ok(viewModel);
        }

        [NonAction]
        private async Task<IEnumerable<ProductsProductViewModel>> PrepareProducts(IQueryable<ProductsProductViewModel> unorderedProducts, int page = 1, int onPage = 20, ProductSorting sorting = ProductSorting.NewerFirst)
        {
            int totalCount = unorderedProducts.Count();
            int totalPages = (int)Math.Ceiling((double)totalCount / onPage);
            if (page > totalPages)
            {
                page = totalPages;
            }
            if (page <= 0)
            {
                page = 1;
            }
            if (onPage > 50)
            {
                onPage = 50;
            }
            if (onPage < 5)
            {
                onPage = 5;
            }

            IOrderedQueryable<ProductsProductViewModel> orderedProducts;

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
                    throw new Exception();
            }

            return orderedProducts.Skip((page - 1) * onPage).Take(onPage);
        }

        [HttpPost("add")]
        [Authorize]
        //TODO: Add photo support
        public async Task<IActionResult> AddMainCategory(AddProductViewModel product)
        {
            string? username = HttpContext.User.Identity?.Name;
            Profile? user = await resourceDbContext.Profiles.FirstOrDefaultAsync(p => p.Username == username);
            if (user == null)
            {
                return BadRequest();
            }
            Category? category = await resourceDbContext.Categories.FirstOrDefaultAsync(c => c.Id.ToString() == product.CategoryId);
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
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAsync(string guid)
        {
            Product? product = await resourceDbContext.Products.Include(p => p.User).FirstOrDefaultAsync(p => p.Id.ToString() == guid);
            if(product == null)
            {
                return BadRequest("No product with such id exists");
            }
            if (!HttpContext.User.IsInRole("Admin") && HttpContext.User.Identity.Name != product.User.Username)
            {
                return Unauthorized();
            }
            resourceDbContext.Products.Remove(product);
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
