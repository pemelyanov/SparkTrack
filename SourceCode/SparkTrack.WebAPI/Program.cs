using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SparkTrack.Core.AutofacModules;
using SparkTrack.DataAccess.EFCore;
using SparkTrack.DataAccess.EFCore.AutofacModules;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory(RegisterServices));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

void RegisterServices(ContainerBuilder container)
{
    container.Register(_ => new DbContextOptionsBuilder<SparkTrackDbContext>()
            .UseInMemoryDatabase("SparkTrack").Options)
        .SingleInstance();
    container.RegisterModule<CoreModule>();
    container.RegisterModule<DataAccessEFModule>();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();