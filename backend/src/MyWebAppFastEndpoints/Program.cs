using MyWebAppFastEndpoints.Infrastructure;

var bld = WebApplication.CreateBuilder();

bld.AddSerilogLogging();

bld.Services
    .AddApplicationServices()
    .AddPersistence(bld.Configuration, bld.Environment)
    .AddJwtAuthentication(bld.Configuration);

var app = bld.Build();

app.UseApplicationPipeline();
app.SeedAdminUser();
app.UseSerilogShutdown();

app.Run();

// ReSharper disable once ClassNeverInstantiated.Global
public partial class Program;

