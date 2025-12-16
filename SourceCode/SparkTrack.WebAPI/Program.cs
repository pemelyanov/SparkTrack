using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SparkTrack.Core.AutofacModules;
using SparkTrack.DataAccess.EFCore;
using SparkTrack.DataAccess.EFCore.AutofacModules;
using SparkTrack.WebAPI.AutofacModules;
using SparkTrack.WebAPI.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory(RegisterServices));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument();

void RegisterServices(ContainerBuilder container)
{
    container.Register(
            _ => new DbContextOptionsBuilder<SparkTrackDbContext>()
                .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
                .Options
        )
        .SingleInstance();
    container.RegisterModule<WebAPIModule>();
    container.RegisterModule<CoreModule>();
    container.RegisterModule<DataAccessEFModule>();
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

app.UseAuthorization();

app.UseMiddleware<AuthorizationServiceMiddleware>();

app.MapControllers();

app.Services.GetRequiredService<SparkTrackDbContext>().Database.EnsureCreated();

app.Run();