using ECommerce1.Models;
using ECommerce1.Models.ViewModels;
using ECommerce1.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AuthUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        private readonly TokenGenerator tokenGenerator;
        private readonly RoleGenerator roleGenerator;

        private readonly AccountDbContext accountDbContext;
        private readonly ResourceDbContext resourceDbContext;

        private readonly IConfiguration configuration;

        public readonly IValidator<RegistrationCredentials> regVal;
        public readonly IValidator<LoginCredentials> logVal;

        public AuthController(UserManager<AuthUser> _userManager,
            RoleManager<IdentityRole> _roleManager,
            TokenGenerator _tokenGenerator,
            RoleGenerator _roleGenerator,
            AccountDbContext _accountDbContext,
            ResourceDbContext _resourceDbContext,
            IConfiguration _configuration,
            IValidator<RegistrationCredentials> _regVal,
            IValidator<LoginCredentials> _logVal
            )
        {
            this.userManager = _userManager;
            this.roleManager = _roleManager;
            this.tokenGenerator = _tokenGenerator;
            this.roleGenerator = _roleGenerator;
            this.accountDbContext = _accountDbContext;
            this.resourceDbContext = _resourceDbContext;
            this.configuration = _configuration;
            this.regVal = _regVal;
            this.logVal = _logVal;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthenticationResponse>> LoginAsync(LoginCredentials loginDto)
        {
            ValidationResult result = await logVal.ValidateAsync(loginDto);
            if (!result.IsValid)
            {
                return BadRequest(result.Errors);
            }

            var user = await userManager.FindByEmailAsync(loginDto.EmailOrUsername);
            if (user == null)
            {
                user = await userManager.FindByNameAsync(loginDto.EmailOrUsername);
                if(user == null)
                {
                    return BadRequest();
                }
            }
            if (!await userManager.CheckPasswordAsync(user, loginDto.Password)) return BadRequest();
            string role = (await userManager.GetRolesAsync(user))[0];
            var accessToken = tokenGenerator.GenerateAccessToken(user, role);
            var refreshToken = tokenGenerator.GenerateRefreshToken();
            accountDbContext.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                ExpiresAt = DateTime.Now.Add(tokenGenerator.Options.RefreshExpiration),
                AppUserId = user.Id
            });
            await accountDbContext.SaveChangesAsync();

            var response = new AuthenticationResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            return Ok(response);
        }

        [HttpPost("registration")]
        public async Task<ActionResult<AuthenticationResponse>> RegistrationAsync(RegistrationCredentials registrationDto)
        {
            string roleString = "User";
            if(registrationDto.Email.Length > 14)
            {
                if (registrationDto.Email.EndsWith("@itstep.edu.az"))
                {
                    roleString = "Admin";
                }
            }
            ValidationResult result = await regVal.ValidateAsync(registrationDto);
            if (!result.IsValid)
            {
                return BadRequest(result.Errors);
            }
            if(await userManager.FindByNameAsync(registrationDto.Username) != null)
            {
                return BadRequest("User with such username does already exist");
            }
            if(await userManager.FindByEmailAsync(registrationDto.Email) != null)
            {
                return BadRequest("User with such e-mail address does already exist");
            }
            if(userManager.Users.Any(u => u.PhoneNumber == registrationDto.PhoneNumber))
            {
                return BadRequest("User with such phone number does already exist");
            }
            City? city = resourceDbContext.Cities.FirstOrDefault(c => c.Id.ToString() == registrationDto.CityId);
            if (city == null)
            {
                if(roleString != "Admin")
                {
                    return BadRequest("Incorrect city");
                }
                city = null;
            }

            AuthUser? user = new()
            {
                Email = registrationDto.Email,
                UserName = registrationDto.Username,
                PhoneNumber = registrationDto.PhoneNumber
            };

            IdentityResult createResult = await userManager.CreateAsync(user, registrationDto.Password);
            if (!createResult.Succeeded)
            {
                return BadRequest(createResult.Errors);
            }

            AuthUser authUser = await userManager.FindByNameAsync(user.UserName);
            IdentityRole authRole = await roleManager.FindByNameAsync(roleString);
            if(authRole == null)
            {
                await roleGenerator.AddDefaultRoles(roleManager);
                authRole = await roleManager.FindByNameAsync(roleString);
            }

            IdentityResult addRole = await userManager.AddToRoleAsync(authUser, authRole.Name);
            if (!addRole.Succeeded)
            {
                await userManager.DeleteAsync(authUser);
                return BadRequest();
            }

            Profile profile = new()
            {
                AuthId = user.Id,
                Username = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FirstName = registrationDto.FirstName,
                LastName = registrationDto.LastName,
                City = city,
                ProfilePictureURL = configuration["Links:Site"] + "images/default.png",
                PreviewProfilePictureURL = configuration["Links:Site"] + "images/default.png",
                Products = new List<Product>()
            };

            resourceDbContext.Profiles.Add(profile);
            await resourceDbContext.SaveChangesAsync();
            string role = (await userManager.GetRolesAsync(user))[0];
            string? accessToken = tokenGenerator.GenerateAccessToken(user, role);
            string? refreshToken = tokenGenerator.GenerateRefreshToken();

            accountDbContext.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                ExpiresAt = DateTime.Now.Add(tokenGenerator.Options.RefreshExpiration),
                AppUserId = user.Id
            });
            accountDbContext.SaveChanges();

            var response = new AuthenticationResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
            return Ok(response);
        }

        [HttpGet("refresh/{oldRefreshToken}")]
        public async Task<ActionResult<AuthenticationResponse>> RefreshAsync(string oldRefreshToken)
        {
            RefreshToken? token = await accountDbContext.RefreshTokens.FindAsync(oldRefreshToken);

            if (token == null)
                return BadRequest();

            accountDbContext.RefreshTokens.Remove(token);

            if (token.ExpiresAt < DateTime.Now)
                return BadRequest();

            AuthUser? user = await userManager.FindByIdAsync(token.AppUserId);
            string role = (await userManager.GetRolesAsync(user))[0];
            string accessToken = tokenGenerator.GenerateAccessToken(user, role);
            string refreshToken = tokenGenerator.GenerateRefreshToken();

            accountDbContext.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                ExpiresAt = DateTime.Now.Add(tokenGenerator.Options.RefreshExpiration),
                AppUserId = user.Id
            });
            accountDbContext.SaveChanges();

            AuthenticationResponse response = new()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
            return Ok(response);
        }

        [HttpGet("logout/{refreshToken}")]
        public async Task<IActionResult> LogoutAsync(string refreshToken)
        {
            var token = accountDbContext.RefreshTokens.Find(refreshToken);
            if (token != null)
            {
                accountDbContext.RefreshTokens.Remove(token);
                await accountDbContext.SaveChangesAsync();
            }
            return NoContent();
        }

        [HttpDelete("delete/{username}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAsync(string username)
        {
            AuthUser authUser = await userManager.FindByNameAsync(username);
            Profile? profile = await resourceDbContext.Profiles.FirstOrDefaultAsync(p => p.Username == username);
            if(authUser != null)
            {
                await userManager.DeleteAsync(authUser);
            }
            if(profile != null)
            {
                resourceDbContext.Profiles.Remove(profile);
                await resourceDbContext.SaveChangesAsync();
            }
            return NoContent();
        }
    }
}
