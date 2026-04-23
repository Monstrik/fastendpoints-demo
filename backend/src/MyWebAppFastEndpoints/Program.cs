var bld = WebApplication.CreateBuilder();

bld.Services
    .AddApplicationServices()
    .AddPersistence(bld.Configuration, bld.Environment)
    .AddJwtAuthentication(bld.Configuration);

var app = bld.Build();

app.UseApplicationPipeline();
app.SeedAdminUser();

app.Run();

// ReSharper disable once ClassNeverInstantiated.Global
public partial class Program;

