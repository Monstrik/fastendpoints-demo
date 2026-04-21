using FastEndpoints;

var bld = WebApplication.CreateBuilder();
bld.Services.AddFastEndpoints();
bld.Services.AddSingleton<IUserStore, InMemoryUserStore>();

var app = bld.Build();
app.UseFastEndpoints();
app.Run();