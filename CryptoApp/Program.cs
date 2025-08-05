using CryptoApp.Models;
using CryptoApp.Services;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Učitaj AppSettings
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

// Registruj servise
builder.Services.AddScoped<CryptoService>();
builder.Services.AddHostedService<FileWatcherService>();
builder.Services.AddTransient<TransferService>();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 500 * 1024 * 1024; // 500 MB limit, prilagodi po potrebi
});


// Razor Pages
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();



app.Run(); // OVO MORA BITI POSLEDNJE — sve posle ovoga se neće izvršiti
