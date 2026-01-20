using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using SparkTrack.Authentication.Core.Models;
using SparkTrack.Authentication.DataAccess.EFCore.AutofacModules;
using SparkTrack.Authentication.WebAPI.Extensions;
using SparkTrack.Core.AutofacModules;
using SparkTrack.Core.Seeding;
using SparkTrack.DataAccess.EFCore;
using SparkTrack.DataAccess.EFCore.AutofacModules;
using SparkTrack.WebAPI.AutofacModules;
using SparkTrack.WebAPI.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory(RegisterServices));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 20L * 1024 * 1024 * 1024;
});

builder.Services.AddOpenApiDocument();


var jwtConfiguration = new JwtConfiguration();
builder.Configuration.Bind("JwtConfiguration", jwtConfiguration);

builder.Services
    .AddJwtConfiguration(jwtConfiguration)
    .AddAccessTokenGenerator()
    .AddRefreshTokenGenerator()
    .AddRefreshTokenValidator()
    .AddRefreshTokenStorageConfiguration()
    .AddRefreshTokensService<Guid>()
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddDefaultJwtBearer(
        jwtConfiguration,
        config: options =>
        {
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];

                    // If the request is for our hub...
                    var path = context.HttpContext.Request.Path;
                    if (
                        !string.IsNullOrEmpty(accessToken)
                        && path.StartsWithSegments("/hub")
                    )
                    {
                        // Read the token out of the query string
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        }
    );

void RegisterServices(ContainerBuilder container)
{
    var isDevelopment = true;
    
    #if !DEBUG
    isDevelopment = false;
    #endif
    
    container.Register(
            _ => new DbContextOptionsBuilder<SparkTrackDbContext>()
                .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
                .Options
        )
        .SingleInstance();
    container.RegisterModule<WebAPIModule>();
    container.RegisterModule(new CoreModule(isDevelopment));
    container.RegisterModule<DataAccessEFModule>();
    container.RegisterModule<AuthenticationDataAccessEFCoreModule>();// TODO: Для консистентности надо бы на экстеншны для ServiceCollection переделать
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<AuthorizationServiceMiddleware>();

app.MapControllers();

var database = app.Services.GetRequiredService<SparkTrackDbContext>().Database;

//database.EnsureDeleted();
database.EnsureCreated();

var seeders = app.Services.GetServices<IDataSeeder>();

foreach (var seeder in seeders)
    await seeder.SeedAsync();

app.Run();