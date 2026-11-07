using ECommerceCsvImport.Models;
using ECommerceCsvImport.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceCsvImport.Controllers
{
    // Контроллер страниц товаров и импорта.
    // Сам файл не читает и данные не проверяет - это делает ProductImportService.
    public class ProductsController : Controller
    {
        private readonly IProductRepository _repository;
        private readonly IProductImportService _importService;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(IProductRepository repository, IProductImportService importService,
            IWebHostEnvironment environment)
        {
            _repository = repository;
            _importService = importService;
            _environment = environment;
        }

        // Список товаров с поиском, фильтром и сортировкой
        public IActionResult Index(string? search, string? category, string? sort)
        {
            IReadOnlyList<Product> allProducts = _repository.GetAll();

            ProductsIndexViewModel model = new ProductsIndexViewModel();
            model.Products = ProductCatalogQuery.Apply(allProducts, search, category, sort);
            model.Categories = ProductCatalogQuery.GetCategories(allProducts);
            model.AllProductsCount = allProducts.Count;
            model.Search = search;
            model.Category = category;
            model.Sort = sort;

            return View(model);
        }

        // Страница с подробной информацией о товаре
        public IActionResult Details(string id)
        {
            Product? product = _repository.GetBySku(id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        [HttpGet]
        public IActionResult Import()
        {
            return View(CreateImportModel());
        }

        // Импорт по пути к файлу
        [HttpPost]
        [ActionName("Import")]
        [ValidateAntiForgeryToken]
        public IActionResult ImportPost(string? filePath)
        {
            ImportViewModel model = CreateImportModel();
            model.FilePath = filePath ?? "";

            if (string.IsNullOrWhiteSpace(filePath))
            {
                model.Result = CreateErrorResult(ImportMessages.FilePathRequired);
                return View("Import", model);
            }

            model.SourceName = filePath;

            try
            {
                model.Result = _importService.ImportFromCsv(GetFullPath(filePath));
            }
            catch (Exception ex)
            {
                // Любая непредвиденная ошибка показывается на странице, приложение не падает
                model.Result = CreateErrorResult(ImportMessages.UnexpectedError + ": " + ex.Message);
            }

            return View("Import", model);
        }

        // Импорт файла, загруженного через браузер
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upload(IFormFile? file)
        {
            ImportViewModel model = CreateImportModel();

            if (file == null || file.Length == 0)
            {
                model.Result = CreateErrorResult(ImportMessages.FileNotSelected);
                return View("Import", model);
            }

            if (Path.GetExtension(file.FileName).ToLower() != ".csv")
            {
                model.Result = CreateErrorResult(ImportMessages.WrongExtension);
                return View("Import", model);
            }

            if (file.Length > ImportConstants.MaxUploadFileSize)
            {
                model.Result = CreateErrorResult(ImportMessages.FileTooLarge);
                return View("Import", model);
            }

            model.SourceName = file.FileName;

            try
            {
                using (Stream stream = file.OpenReadStream())
                {
                    model.Result = _importService.ImportFromStream(stream);
                }
            }
            catch (Exception ex)
            {
                model.Result = CreateErrorResult(ImportMessages.UnexpectedError + ": " + ex.Message);
            }

            return View("Import", model);
        }

        // Скачивание всех товаров в виде CSV-файла
        public IActionResult Export()
        {
            byte[] bytes = ProductCsvExporter.ToCsvBytes(_repository.GetAll());
            return File(bytes, "text/csv", "products_export.csv");
        }

        // Удаление всех товаров из памяти
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            _repository.Clear();
            TempData["Message"] = "Список товаров очищен";
            return RedirectToAction("Index");
        }

        // Путь вида "/csv/file.csv" считаем относительно папки wwwroot,
        // полный путь (например, C:\data\products.csv) используем как есть
        private string GetFullPath(string filePath)
        {
            string path = filePath.Trim().Trim('"');

            if (Path.IsPathFullyQualified(path) && System.IO.File.Exists(path))
                return path;

            string relativePath = path.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(_environment.WebRootPath, relativePath);
        }

        private ImportViewModel CreateImportModel()
        {
            ImportViewModel model = new ImportViewModel();

            // Список примеров CSV из папки wwwroot/csv для быстрого выбора
            string csvFolder = Path.Combine(_environment.WebRootPath, "csv");
            if (Directory.Exists(csvFolder))
            {
                foreach (string file in Directory.GetFiles(csvFolder, "*.csv", SearchOption.AllDirectories))
                {
                    string relative = Path.GetRelativePath(_environment.WebRootPath, file).Replace('\\', '/');
                    model.SampleFiles.Add("/" + relative);
                }
                model.SampleFiles.Sort();
            }

            return model;
        }

        private static ImportResult CreateErrorResult(string message)
        {
            ImportResult result = new ImportResult();
            result.AddError(ImportConstants.FileErrorRowNumber, message);
            return result;
        }
    }
}
