using FastEndpoints;

var bld = WebApplication.CreateBuilder();
bld.Services.AddFastEndpoints();
bld.Services.AddSingleton<IUserStore, InMemoryUserStore>();

var app = bld.Build();
app.UseFastEndpoints();
app.Run();

// ReSharper disable once ClassNeverInstantiated.Global
public partial class Program;
