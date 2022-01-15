using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using WebApi.Authorization;
using WebApi.Helpers;
using WebApi.Services;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information($"Starting API at {DateTime.Now}");

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console()
        .ReadFrom.Configuration(ctx.Configuration));

    // add services to DI container
    {
        IServiceCollection services = builder.Services;
        IWebHostEnvironment env = builder.Environment;
        
        services.AddDbContext<DataContext>();

        services.AddCors();
        services.AddControllers();

        // Only set up Swagger in Development Mode
        if (env.IsDevelopment())
        {
            // Swagger Setup
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = ".NET 6 Registration and Login with JWT API",
                    Description = "API for registration and log using JWT auth.",
                    Contact = new OpenApiContact
                    {
                        Name = "Robert Cato",
                        Email = "saiwolf@swmnu.net",
                        Url = new Uri("https://github.com/saiwolf"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Licensed under the MIT License",
                        Url = new Uri("https://github.com/saiwolf/dotnet-6-registration-login-api/blob/main/LICENSE")
                    },
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization using the Bearer scheme."
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

                string xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });
        }

        // configure automapper with all automapper profiles from this assembly
        services.AddAutoMapper(typeof(Program));

        // configure strongly typed settings object
        services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

        // configure DI for application services
        services.AddScoped<IJwtUtils, JwtUtils>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEmailService, EmailService>();
    }

    WebApplication app = builder.Build();

    if (builder.Environment.IsDevelopment())
    {
        // migrate any database changes on startup (includes initial db creation)
        using IServiceScope scope = app.Services.CreateScope();
        DataContext dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
        dataContext.Database.Migrate();
    }
    

    // configure HTTP request pipeline
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // global cors policy
        app.UseCors(x => x
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());

        // global error handler
        app.UseMiddleware<ErrorHandlerMiddleware>();

        // custom jwt auth middleware
        app.UseMiddleware<JwtMiddleware>();

        app.MapControllers();
    }

    app.Run("http://localhost:4000");
}
catch (Exception ex)
{
    Log.Fatal(ex, ex.Message);
}
finally
{
    Log.Information($"API Shutdown at {DateTime.Now}");
    Log.CloseAndFlush();
}