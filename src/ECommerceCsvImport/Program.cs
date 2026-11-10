using System.Globalization;
using ECommerceCsvImport.Models;
using ECommerceCsvImport.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Репозиторий и сервис регистрируем как Singleton: товары хранятся в памяти
// и должны сохраняться между запросами (при AddScoped они бы терялись).
builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();
builder.Services.AddSingleton<IImportErrorLogger>(
    new FileImportErrorLogger(Path.Combine(builder.Environment.ContentRootPath, "logs", "import-errors.log")));
builder.Services.AddSingleton<IProductImportService, ProductImportService>();

// Русский формат чисел на страницах (например, 45 990)
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("ru-RU");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("ru-RU");

var app = builder.Build();

// Автоматический импорт товаров при запуске приложения.
// Путь к файлу (относительно папки wwwroot) задается в appsettings.json, раздел "Import".
bool autoImport = builder.Configuration.GetValue<bool>("Import:AutoImportOnStartup");
string? importPath = builder.Configuration["Import:FilePath"];
if (autoImport && !string.IsNullOrEmpty(importPath))
{
    string fullPath = Path.Combine(app.Environment.WebRootPath, importPath);
    IProductImportService importService = app.Services.GetRequiredService<IProductImportService>();
    ImportResult startupResult = importService.ImportFromCsv(fullPath);
    app.Logger.LogInformation("Импорт при запуске ({Path}): добавлено {Imported}, ошибок {Errors}",
        importPath, startupResult.ImportedCount, startupResult.ErrorCount);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");

app.Run();
