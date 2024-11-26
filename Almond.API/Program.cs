using Almond.API.Core;
using Almond.API.Data;
using Almond.API.Handlers;
using Almond.API.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var postgresSettings = builder.Configuration.GetSection(nameof(PostgresSettings)).Get<PostgresSettings>();
var connectionString = postgresSettings?.ConnectionString;

builder.Services.AddDbContext<AlmondDbContext>(opt => opt.UseNpgsql(connectionString));
builder.Services.AddAuthorization();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<AlmondDbContext>();

builder.Services.AddScoped<IGetUserClaims, GetUserClaims>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Tournament Bracket API",
        Description = "An ASP.NET Core Web API for generating and managing tournament brackets",
        Contact = new OpenApiContact
        {
            Name = "Mustapha Ojo",
            Email = "ojomustapha@outlook.com"
        }
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services.AddRouting(options => options.LowercaseUrls = true);


var app = builder.Build();

app.UseExceptionHandler();
app.MapIdentityApi<IdentityUser>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.SupportedSubmitMethods();
    options.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapPost("/logout", async (SignInManager<IdentityUser> signInManger,
    [FromBody] object empty) =>
{
    if (empty != null)
    {
        await signInManger.SignOutAsync();
        return Results.Ok();
    }

    return Results.Unauthorized();
}).RequireAuthorization();

app.Run();
