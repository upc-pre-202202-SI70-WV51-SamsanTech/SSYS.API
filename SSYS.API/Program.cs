using System.Runtime.Intrinsics.X86;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SSYS.API.HCM.Domain.Repositories;
using SSYS.API.HCM.Domain.Services;
using SSYS.API.HCM.Persistence.Repositories;
using SSYS.API.HCM.Services;
using SSYS.API.IAM.Authorization.Handlers.Implementations;
using SSYS.API.IAM.Authorization.Handlers.Interfaces;
using SSYS.API.IAM.Authorization.Middleware;
using SSYS.API.IAM.Authorization.Settings;
using SSYS.API.IAM.Domain.Repositories;
using SSYS.API.IAM.Domain.Services;
using SSYS.API.IAM.Interfaces.Internal;
using SSYS.API.IAM.Persistence.Repositories;
using SSYS.API.IAM.Services;
using SSYS.API.Profile.Domain.Repositories;
using SSYS.API.Profile.Domain.Services;
using SSYS.API.Profile.Persistence.Repositories;
using SSYS.API.Profile.Services;
using SSYS.API.SCM.Domain.Repositories;
using SSYS.API.SCM.Domain.Services;
using SSYS.API.SCM.Persistence;
using SSYS.API.SCM.Services;
using SSYS.API.Shared.Domain.Repositories;
using SSYS.API.Shared.Persistence.Contexts;
using SSYS.API.Shared.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Add Database Connection

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(
    option => option.UseMySQL(connectionString)
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors());

// Add lowercase routes
builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// HCM Bounded Context Dependency Injection Configuration

builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

// Profile Bounded Context Dependency Injection Configuration
builder.Services.AddScoped<IProfileRepository, ProfileRespository>();
builder.Services.AddScoped<IProfileService, ProfileService>();

// SCM Bounded Context Dependency Injection
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

// Security Bounded Context Dependency Injection Configuration
builder.Services.AddScoped<IJwtHandler, JwtHandler>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserContextFacade, UserContextFacade>();

// AutoMapper Configuration

builder.Services.AddAutoMapper(
    typeof(SSYS.API.IAM.Mapping.ModelToResourceProfile),
    typeof(SSYS.API.IAM.Mapping.ResourceToModelProfile),
    typeof(SSYS.API.HCM.Mapping.ModelToResourceProfile),
    typeof(SSYS.API.HCM.Mapping.ResourceToModelProfile),
    typeof(SSYS.API.Profile.Mapping.ModelToResourceProfile),
    typeof(SSYS.API.Profile.Mapping.ResourceToModelProfile),
    typeof(SSYS.API.SCM.Mapping.ModelToResourceProduct),
    typeof(SSYS.API.SCM.Mapping.ResourceToModelProduct));

// Swagger Configuration
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "SSYS API",
        Description = "SSYS Web Service",
        TermsOfService = new Uri("https://upc-pre-202202-si70-wv51-samsantech.github.io/landingpage-ssys/tos"),
        Contact = new OpenApiContact
        {
            Name = "SSYS.PE",
            Url = new Uri("https://www.ssys.pe")
        },
        License = new OpenApiLicense
        {
            Name = "Samsan Tech SSYS Resource License",
            Url = new Uri("https://upc-pre-202202-si70-wv51-samsantech.github.io/landingpage-ssys/license")
        }
    });
    options.EnableAnnotations();
});


// AppSettings Configuration
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
    
    
var app = builder.Build();

// Validation for ensuring Database Objects are created

using (var scope = app.Services.CreateScope())
using (var context = scope.ServiceProvider.GetService<AppDbContext>())
{
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("v1/swagger.json", "v1");
        options.RoutePrefix = "swagger";
    });
}

// Configure CORS
app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

// Configure Error Handler Middleware
app.UseMiddleware<ErrorHandlerMiddleware>();


// Configure JWT Handling Middleware
app.UseMiddleware<JwtMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}