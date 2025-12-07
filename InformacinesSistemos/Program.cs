
using InformacinesSistemos.Components;
using InformacinesSistemos.Data;
using InformacinesSistemos.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Jei EF naudodamas:
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

builder.Services.AddScoped<ProtectedLocalStorage>();
builder.Services.AddScoped<IUserService, PgUserService>();

// Jūsų kiti servisai...
builder.Services.AddScoped<SimpleAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<SimpleAuthenticationStateProvider>());

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

// Registracijos ir prisijungimo servisai
builder.Services.AddScoped<IRegistrationService, PgRegistrationService>();
builder.Services.AddScoped<ILoginService, PgLoginService>();
builder.Services.AddScoped<IGameService, PgGamesService>();
builder.Services.AddScoped<ICartService, InMemoryCartService>(); // ← FIXES YOUR CART ERROR
builder.Services.AddScoped<IUserService, PgUserService>(); // ← FIXES YOUR CART ERROR
builder.Services.AddScoped<IReviewService, PgReviewService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
