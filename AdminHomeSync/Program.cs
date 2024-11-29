using AdminHomeSync.Components;
using AdminHomeSync.Components.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<ActivityService>(); //activity page
builder.Services.AddScoped<DeviceService>(); //device page
builder.Services.AddScoped<IUserService, UserService>(); //users page
builder.Services.AddScoped<FirebaseAuthService>(); //signup page
builder.Services.AddHttpClient(); // For handling HTTP requests
builder.Services.AddScoped<ProfileService>(); // Register ProfileService here
builder.Services.AddSingleton<ProfileService>();

// Register NotificationService as Scoped
builder.Services.AddScoped<NotificationService>(); // notification page

// Register HttpClient with a specific base address
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri("https://homesync-3be92-default-rtdb.firebaseio.com/") });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
