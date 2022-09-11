using Azure.Storage.Blobs;
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
    public class ProfileController : ControllerBase
    {
        private readonly ResourceDbContext resourceDbContext;
        private readonly IConfiguration configuration;

        public ProfileController(ResourceDbContext resourceDbContext, IConfiguration configuration)
        {
            this.resourceDbContext = resourceDbContext;
            this.configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult<Profile>> GetOwn()
        {
            var profile = await resourceDbContext.Profiles.FirstOrDefaultAsync(x => x.Username == HttpContext.User.Identity.Name);
            if (profile == null) return NotFound();
            return Ok(profile);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<Profile>> GetByUserNameAsync(string username)
        {
            var profile = await resourceDbContext.Profiles.FirstOrDefaultAsync(x => x.Username == username);
            if (profile == null) return NotFound();
            return Ok(profile);
        }

        [HttpPost("upload")]
        [Authorize]
        public async Task<IActionResult> Upload()
        {
            if (HttpContext.Request.Form.Files.Any())
            {
                foreach (var file in HttpContext.Request.Form.Files)
                {
                    var newName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var blobServiceClient = new BlobServiceClient(configuration.GetConnectionString("BlobStorage"));
                    var containerClient = blobServiceClient.GetBlobContainerClient("uploads");
                    var containerClient2 = blobServiceClient.GetBlobContainerClient("thumbnails");
                    var username = HttpContext.User.Identity.Name;
                    var profile = await resourceDbContext.Profiles.FirstOrDefaultAsync(p => p.Username == username);
                    if (!String.IsNullOrWhiteSpace(profile.ProfilePictureURL))
                    {
                        var oldFileName = profile.ProfilePictureURL.Substring(profile.ProfilePictureURL.LastIndexOf('/') + 1);
                        await containerClient.DeleteBlobIfExistsAsync(oldFileName);
                    }
                    if (!String.IsNullOrWhiteSpace(profile.PreviewProfilePictureURL))
                    {
                        var oldPreviewFileName = profile.PreviewProfilePictureURL.Substring(profile.PreviewProfilePictureURL.LastIndexOf('/') + 1);
                        await containerClient2.DeleteBlobIfExistsAsync(oldPreviewFileName);
                    }
                    await containerClient.UploadBlobAsync(newName, file.OpenReadStream());
                    profile.ProfilePictureURL = configuration["Links:Files:Pictures"] + newName;
                    profile.PreviewProfilePictureURL = configuration["Links:Files:Thumbnails"] + newName;
                    resourceDbContext.Profiles.Update(profile);
                    await resourceDbContext.SaveChangesAsync();
                }
            }
            return Ok();
        }

        [HttpPost("reset")]
        [Authorize]
        public async Task<IActionResult> Reset()
        {
            var blobServiceClient = new BlobServiceClient(configuration.GetConnectionString("BlobStorage"));
            var containerClient = blobServiceClient.GetBlobContainerClient("uploads");
            var containerClient2 = blobServiceClient.GetBlobContainerClient("thumbnails");
            var username = HttpContext.User.Identity.Name;
            var profile = await resourceDbContext.Profiles.FirstOrDefaultAsync(p => p.Username == username);
            if (!String.IsNullOrWhiteSpace(profile.ProfilePictureURL))
            {
                var oldFileName = profile.ProfilePictureURL.Substring(profile.ProfilePictureURL.LastIndexOf('/') + 1);
                await containerClient.DeleteBlobIfExistsAsync(oldFileName);
            }
            if (!String.IsNullOrWhiteSpace(profile.PreviewProfilePictureURL))
            {
                var oldPreviewFileName = profile.PreviewProfilePictureURL.Substring(profile.PreviewProfilePictureURL.LastIndexOf('/') + 1);
                await containerClient2.DeleteBlobIfExistsAsync(oldPreviewFileName);
            }
            profile.ProfilePictureURL = configuration["Links:Site"] + "images/default.png";
            profile.PreviewProfilePictureURL = configuration["Links:Site"] + "images/default.png";
            resourceDbContext.Profiles.Update(profile);
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("reset/{username}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reset(string username)
        {
            var blobServiceClient = new BlobServiceClient(configuration.GetConnectionString("BlobStorage"));
            var containerClient = blobServiceClient.GetBlobContainerClient("uploads");
            var containerClient2 = blobServiceClient.GetBlobContainerClient("thumbnails");
            var profile = await resourceDbContext.Profiles.FirstOrDefaultAsync(p => p.Username == username);
            if(profile == null)
            {
                return BadRequest("User not found");
            }
            if (!String.IsNullOrWhiteSpace(profile.ProfilePictureURL))
            {
                var oldFileName = profile.ProfilePictureURL.Substring(profile.ProfilePictureURL.LastIndexOf('/') + 1);
                await containerClient.DeleteBlobIfExistsAsync(oldFileName);
            }
            if (!String.IsNullOrWhiteSpace(profile.PreviewProfilePictureURL))
            {
                var oldPreviewFileName = profile.PreviewProfilePictureURL.Substring(profile.PreviewProfilePictureURL.LastIndexOf('/') + 1);
                await containerClient2.DeleteBlobIfExistsAsync(oldPreviewFileName);
            }
            profile.ProfilePictureURL = configuration["Links:Site"] + "images/default.png";
            profile.PreviewProfilePictureURL = configuration["Links:Site"] + "images/default.png";
            resourceDbContext.Profiles.Update(profile);
            await resourceDbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
