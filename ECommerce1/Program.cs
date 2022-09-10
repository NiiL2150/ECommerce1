using ECommerce1.Extensions;
using ECommerce1.Models;
using ECommerce1.Models.Validators;
using ECommerce1.Models.ViewModels;
using ECommerce1.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var config = builder.Configuration;

#region Services
services.AddDbContextPool<ResourceDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("ResourcesHost")));
services.AddDbContextPool<AccountDbContext>(options =>
    options.UseSqlServer(config.GetConnectionString("AccountHost")));
services.AddIdentity<AuthUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
    options.SignIn.RequireConfirmedEmail = false;

    options.User.RequireUniqueEmail = true;

    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
}).AddEntityFrameworkStores<AccountDbContext>();

services.AddControllers();
services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
services.AddEndpointsApiExplorer();
services.AddSwagger();
services.AddJwtAuthentication(config["Secret"], new List<string>() { "User", "Admin" });
services.AddScoped<IValidator<RegistrationCredentials>, RegistrationValidator>();
services.AddScoped<IValidator<LoginCredentials>, LoginValidator>();

services.AddAzureClients(builder =>
{
    builder.AddBlobServiceClient(config["ConnectionStrings:BlobStorage:blob"], preferMsi: true);
    builder.AddQueueServiceClient(config["ConnectionStrings:BlobStorage:queue"], preferMsi: true);
});
#endregion

#region Configure
var app = builder.Build();

app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "olx.az API");
});

app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
#endregion