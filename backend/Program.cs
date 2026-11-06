using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using ReadyErp.Api.Infrastructure;
using ReadyErp.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=readyerp.db"));

builder.Services.AddScoped<NumberingService>();
builder.Services.AddScoped<TokenService>();

// Upload storage. Path is configurable so a container can point it at a
// mounted volume; see StorageOptions.
var storage = StorageOptions.Create(builder.Configuration, builder.Environment.ContentRootPath);
builder.Services.AddSingleton(storage);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = TokenService.Issuer,
            ValidAudience = TokenService.Issuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TokenService.SecretKey))
        };
    });
builder.Services.AddAuthorization();

const string CorsPolicy = "AllowFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbSeeder.Seed(db);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(CorsPolicy);

// Serve uploaded product images from the configured storage path. Anonymous
// by design: <img> tags cannot send an Authorization header.
//
// Guarded: if no writable storage could be resolved the API still runs, it
// simply cannot serve or accept images.
if (Directory.Exists(storage.UploadPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(storage.UploadPath),
        RequestPath = storage.RequestPath
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
