using System.Globalization;
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
