using RaceDay.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. ADD SERVICES TO THE CONTAINER (DI)
// ==========================================

// Add MVC services
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// Register your custom services
builder.Services.AddScoped<RaceDayApiService>();
// (Uncomment the line below instead if RaceDayApiService uses an interface)
// builder.Services.AddScoped<IRaceDayApiService, RaceDayApiService>();

// *** THIS WAS MISSING - THIS FIXES YOUR ERROR ***
builder.Services.AddScoped<IUserSessionService, UserSessionService>();

// Session management for tracking authenticated user ID and Role
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register typed HttpClient with interface mapping
builder.Services.AddHttpClient<IRaceDayApiClient, RaceDayApiClient>(client =>
{
    var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
        ?? builder.Configuration["ApiBaseUrl"]
        ?? "http://localhost:5000/";
    client.BaseAddress = new Uri(apiBaseUrl.EndsWith("/") ? apiBaseUrl : apiBaseUrl + "/");
});

// ==========================================
// 2. BUILD THE APPLICATION
// ==========================================
var app = builder.Build();

// ==========================================
// 3. CONFIGURE THE HTTP REQUEST PIPELINE
// ==========================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session middleware must be placed AFTER UseRouting and BEFORE UseAuthorization/MapControllerRoute
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
