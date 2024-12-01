using AdminHomeSync.Components;
using AdminHomeSync.Components.Services;
using Blazored.LocalStorage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register required services
builder.Services.AddSingleton<ActivityService>(); // Activity page
builder.Services.AddScoped<DeviceService>(); // Device page
builder.Services.AddScoped<IUserService, UserService>(); // Users page
builder.Services.AddScoped<FirebaseAuthService>(); // Signup/Login
builder.Services.AddScoped<ProfileService>(); // Profile page
builder.Services.AddScoped<NotificationService>(); // Notification service
builder.Services.AddBlazoredLocalStorage(); // Local storage service\

// Register HttpClient with a specific base address (Firebase)
builder.Services.AddHttpClient<FirebaseAuthService>(client =>
{
    client.BaseAddress = new Uri("https://homesync-3be92-default-rtdb.firebaseio.com/");
});


// If you need a generic HttpClient elsewhere, register it explicitly
builder.Services.AddHttpClient();

// Build the app
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
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
