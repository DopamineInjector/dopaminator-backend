using Microsoft.EntityFrameworkCore;
using Dopaminator.Models;
using Dopaminator.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var jwtKey = builder.Configuration["JwtSettings:SecretKey"];
var imagePath = Path.Combine(Directory.GetCurrentDirectory(), builder.Configuration["MintableSettings:Path"]);
var blockchainUrl = builder.Configuration["Blockchain:URL"];
var adminUuid = builder.Configuration["Admin:GUID"];

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped(sp => new MintableService(sp.GetRequiredService<AppDbContext>(), imagePath));
builder.Services.AddScoped(sp => new BlockchainService(blockchainUrl ?? "http://localhost:8083", adminUuid));
builder.Services.AddScoped(sp => new ImageService());
builder.Services.AddScoped(sp => new PostService(sp.GetRequiredService<AppDbContext>()));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync("{\"message\": \"There was an error authorizing you request\"}");
        }
    };
});

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors("AllowSpecificOrigin");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseStaticFiles();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    var dbContext = services.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
    var mintableService = services.GetRequiredService<MintableService>();
    await mintableService.Init();
    var blockchainService = services.GetRequiredService<BlockchainService>();
    await blockchainService.Init();
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during initialization.");
}

app.Run();
