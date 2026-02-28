using Blazing.Mvvm;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using MyRecipes.Data;
using MyRecipes.UI.Components;
using MyRecipes.UI.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSerilog();
builder.Services.AddServices();
builder.Services.AddMudServices();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMvvm(options =>
{
    options.HostingModelType = BlazorHostingModelType.Server;
    options.ParameterResolutionMode = ParameterResolutionMode.ViewAndViewModel;
});
builder.Services.AddDbContextFactory<RecipeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Error", true);

app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();