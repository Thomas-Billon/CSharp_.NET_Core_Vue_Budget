using Budget.Server.Data;
using Budget.Server.Data.Startup;
using Budget.Server.Mappers;
using Budget.Server.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

builder.ConfigureBuilder()
    .ConfigureCors()
    .ConfigureDbContext()
    .ConfigureServices();

var app = builder.Build();

await app.ConfigureApp()
    .ConfigureDbMigration();

app.Run();


public static class ProgramExtensions
{
    public static WebApplicationBuilder ConfigureBuilder(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddOpenApi();

        return builder;
    }

    public static WebApplicationBuilder ConfigureCors(this WebApplicationBuilder builder)
    {
        string[] allowedOrigins = builder.Configuration.GetValue<string>("AllowedOrigins")?.Split(';') ?? [];

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
        return builder;
    }

    public static WebApplicationBuilder ConfigureDbContext(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ApplicationDbContext>(
            options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
        );

        return builder;
    }
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IDbInitializerService, DbInitializerService>();

        builder.Services.AddScoped<TransactionService>();

        builder.Services.AddSingleton<TransactionMapper>();

        return builder;
    }

    public static WebApplication ConfigureApp(this WebApplication app)
    {
        app.UseHttpsRedirection();

        app.UseDefaultFiles();
        app.MapStaticAssets();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseCors();

        app.UseAuthorization();

        app.MapControllers();

        app.MapFallbackToFile("/index.html");

        return app;
    }

    public static async Task<WebApplication> ConfigureDbMigration(this WebApplication app)
    {
        using (IServiceScope serviceScope = app.Services.CreateScope())
        {
            IServiceProvider services = serviceScope.ServiceProvider;

            await services.GetRequiredService<IDbInitializerService>().Init(services);
        }

        return app;
    }
}
