using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ProductService.Authentication;
using ProductService.Authorization;
using ProductService.Constants;
using ProductService.Data;
using ProductService.Interceptors;
using ProductService.Jobs;
using ProductService.Mapping;
using ProductService.Middleware;
using ProductService.Models;
using ProductService.Repositories;
using ProductService.Repositories.Generic;
using ProductService.Repositories.UnitOfWork;
using ProductService.Services;
using ProductService.Services.Cache;
using ProductService.Validators;
using Serilog;
using StackExchange.Redis;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

#region Configure Serilog

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(
        "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();

builder.Host.UseSerilog();

#endregion

#region Add Services

// Controllers
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// Problem Details
builder.Services.AddProblemDetails();

// API Explorer
builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Product Catalog API",
        Version = "v1",
        Description = "Enterprise Product Catalog REST API"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database


builder.Services.AddDbContext<ProductDbContext>((serviceProvider, options) =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });

    options.AddInterceptors(
        serviceProvider.GetRequiredService<AuditInterceptor>());
});
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ProductDbContext>("SQL Server");


builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddSingleton<AuditInterceptor>();
// Dependency Injection
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService.Services.ProductService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IReviewService, ReviewService>();
// AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped(typeof(IGenericRepository<>),
                           typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});


builder.Services.AddMemoryCache();
builder.Services.AddHangfire(config =>
{
    config.UseSqlServerStorage(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddHangfireServer();
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService("ProductCatalogAPI"))

            .AddAspNetCoreInstrumentation()

            .AddHttpClientInstrumentation()

            .AddConsoleExporter();
    });

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

// Fluent Validation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<ProductValidator>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

#endregion

#region JWT Authentication

var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException("JWT Key is missing from appsettings.json");
}

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//.AddJwtBearer(options =>
//{
//    options.RequireHttpsMetadata = false;
//    options.SaveToken = true;

//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,

//        ValidIssuer = builder.Configuration["Jwt:Issuer"],
//        ValidAudience = builder.Configuration["Jwt:Audience"],

//        IssuerSigningKey = new SymmetricSecurityKey(
//            Encoding.UTF8.GetBytes(jwtKey))
//    };
//});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey))
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("JWT ERROR: " + context.Exception.Message);
            return Task.CompletedTask;
        },

        OnTokenValidated = context =>
        {
            Console.WriteLine("JWT VALIDATED");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    // Product
    options.AddPolicy(Permissions.ProductView,
        policy => policy.Requirements.Add(new PermissionRequirement(Permissions.ProductView)));

    options.AddPolicy(Permissions.ProductCreate,
        policy => policy.Requirements.Add(new PermissionRequirement(Permissions.ProductCreate)));

    options.AddPolicy(Permissions.ProductUpdate,
        policy => policy.Requirements.Add(new PermissionRequirement(Permissions.ProductUpdate)));

    options.AddPolicy(Permissions.ProductDelete,
        policy => policy.Requirements.Add(new PermissionRequirement(Permissions.ProductDelete)));

    // Category
    options.AddPolicy(Permissions.CategoryCreate,
        policy => policy.Requirements.Add(new PermissionRequirement(Permissions.CategoryCreate)));

    options.AddPolicy(Permissions.CategoryUpdate,
        policy => policy.Requirements.Add(new PermissionRequirement(Permissions.CategoryUpdate)));

    options.AddPolicy(Permissions.CategoryDelete,
        policy => policy.Requirements.Add(new PermissionRequirement(Permissions.CategoryDelete)));

    // Dashboard
    options.AddPolicy(Permissions.DashboardView,
        policy => policy.Requirements.Add(new PermissionRequirement(Permissions.DashboardView)));

    // Audit
    options.AddPolicy(Permissions.AuditView,
        policy => policy.Requirements.Add(new PermissionRequirement(Permissions.AuditView)));
    
});
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuditService, AuditService>();

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddScoped<IEmailService, EmailService>();
#endregion
builder.Services.AddScoped<EmailJobs>();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("FixedPolicy", opt =>
    {
        opt.PermitLimit = 5;

        opt.Window = TimeSpan.FromMinutes(1);

        opt.QueueLimit = 0;

        opt.QueueProcessingOrder =
            QueueProcessingOrder.OldestFirst;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration =
        builder.Configuration.GetConnectionString("Redis");
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration =
        ConfigurationOptions.Parse(
            builder.Configuration.GetConnectionString("Redis")!);

    configuration.AbortOnConnectFail = false;

    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var db = services.GetRequiredService<ProductDbContext>();

        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

#region Configure Middleware

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestTracingMiddleware>();
app.UseSerilogRequestLogging();


if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}



app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseRouting();

app.UseMiddleware<SwaggerBasicAuthMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Catalog API v1");
        c.RoutePrefix = "swagger";

    });
    app.UseAuthentication();
    app.UseAuthorization();

}
app.UseRateLimiter();

app.UseCors("AllowAll");

app.MapControllers();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[]
    {
        new ProductService.Authorization.HangfireDashboardAuthorizationFilter()
    }
});
app.MapHangfireDashboard();

app.MapHealthChecks("/health");

#endregion
app.Run();