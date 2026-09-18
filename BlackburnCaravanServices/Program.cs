using Azure.Identity;
using Azure.Storage.Blobs;
using BlackburnCaravanServices.Data;
using BlackburnCaravanServices.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Authentication
builder.Services
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(
        builder.Configuration.GetSection("AzureAd")
    );

// Add services to the container.
builder.Services.AddRazorPages();

// Database
var connectionString =
    builder.Configuration["SQL_CONNECTION_STRING"]
    ?? throw new InvalidOperationException(
        "SQL_CONNECTION_STRING was not found."
    );

builder.Services.AddDbContext<CaravanDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null
            );
        }
    )
);

// Azure Blob Storage
var storageAccountName =
    builder.Configuration["AzureStorage:AccountName"];

if (!string.IsNullOrWhiteSpace(storageAccountName))
{
    builder.Services.AddSingleton(
        new BlobServiceClient(
            new Uri(
                $"https://{storageAccountName}.blob.core.windows.net"
            ),
            new DefaultAzureCredential()
        )
    );
}

builder.Services.AddScoped<CaravanImageStorageService>();

// Azure Container Apps terminates HTTPS at the ingress proxy.
// Use the forwarded headers so ASP.NET Core knows that the
// original request from the browser used HTTPS.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
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

app.Run();