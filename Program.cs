using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Prisma.Data;
using Prisma.Services;
using PrismaNews.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();


// Add SQLite Database
builder.Services.AddDbContext<PrismaDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add HttpClient and Guardian API Service
builder.Services.AddHttpClient();
builder.Services.AddScoped<GuardianApiService>();
builder.Services.AddScoped<INewsService, GuardianNewsAdapter>();
// Aggiungi queste righe alla sezione di registrazione dei servizi
builder.Services.AddHttpClient<GeminiService>();
builder.Services.AddHttpClient<NewsScraperService>();
builder.Services.AddScoped<GeminiService>();
builder.Services.AddScoped<NewsScraperService>();
builder.Services.AddScoped<NewsService>();
builder.Services.AddWebScraper();  // Aggiunge il servizio di scraping configurato
builder.Services.AddScoped<NewsService>();
builder.Services.AddScoped<GeminiService>();
builder.Services.AddScoped<NarrativeLensService>();

// Add authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

// Add authorization policies if needed
builder.Services.AddAuthorization(options =>
{
    // Example: Require authenticated users for certain areas
    options.AddPolicy("RequireAuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Ensure the database is created
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<PrismaDbContext>();
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while creating the database.");
    }
}

app.Run();