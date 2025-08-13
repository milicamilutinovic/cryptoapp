using CryptoApp.Models;
using CryptoApp.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Učitaj AppSettings
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

// Registruj servise
builder.Services.AddScoped<CryptoService>();
builder.Services.AddScoped<EncryptionHelper>();
builder.Services.AddHostedService<FileWatcherService>();
builder.Services.AddTransient<TransferService>();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 500 * 1024 * 1024; // 500 MB limit, prilagodi po potrebi
});


// Razor Pages
builder.Services.AddRazorPages();
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));


var app = builder.Build();

var env = app.Environment;

// Configure HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(env.WebRootPath, "received")),
    RequestPath = "/received"
});

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();



app.Run(); // OVO MORA BITI POSLEDNJE — sve posle ovoga se neće izvršiti
