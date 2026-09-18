using Azure.Identity;
using Azure.Storage.Blobs;
using BlackburnCaravanServices.Data;
using BlackburnCaravanServices.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Database
var connectionString =
    builder.Configuration["SQL_CONNECTION_STRING"]
    ?? throw new InvalidOperationException(
        "SQL_CONNECTION_STRING was not found."
    );

builder.Services.AddDbContext<CaravanDbContext>(options =>
    options.UseSqlServer(connectionString)
);

// Azure Blob Storage
var storageAccountName = builder.Configuration["AzureStorage:AccountName"];

if (!string.IsNullOrWhiteSpace(storageAccountName))
{
    builder.Services.AddSingleton(
        new BlobServiceClient(
            new Uri($"https://{storageAccountName}.blob.core.windows.net"),
            new DefaultAzureCredential()
        )
    );
}

builder.Services.AddScoped<CaravanImageStorageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
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

app.Run();