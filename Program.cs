using Microsoft.EntityFrameworkCore;
using PrismaNews.Data;
using PrismaNews.services;
using PrismaNews.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Configure SQLite instead of SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
SqliteConfiguration.ConfigureDatabase(builder.Services, connectionString);

// Register HTTP clients
builder.Services.AddHttpClient<INewsService, NewsApiService>();
builder.Services.AddHttpClient<Gemini2FlashService>();

// Register application services
builder.Services.AddScoped<INewsService, NewsApiService>();
builder.Services.AddScoped<IArticleAnalysisService, Gemini2FlashService>();

// Register repositories
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();