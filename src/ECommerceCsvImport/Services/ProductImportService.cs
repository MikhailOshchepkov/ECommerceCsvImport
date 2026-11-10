using System.Globalization;
using System.Text;
using ECommerceCsvImport.Models;

namespace ECommerceCsvImport.Services
{
    // Сервис импорта товаров из CSV.
    // Читает файл, разбирает строки, проверяет данные и добавляет корректные товары в репозиторий.
    public class ProductImportService : IProductImportService
    {
        private readonly IProductRepository _repository;
        private readonly IImportErrorLogger? _errorLogger;

        public ProductImportService(IProductRepository repository, IImportErrorLogger? errorLogger = null)
        {
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            _repository = repository;
            _errorLogger = errorLogger;
        }

        // Импорт из файла: проверяем, что файл есть, читаем его и передаем строки дальше
        public ImportResult ImportFromCsv(string filePath)
        {
            ImportResult result;

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                result = new ImportResult();
                result.AddError(ImportConstants.FileErrorRowNumber, ImportMessages.FileNotFound);
            }
            else
            {
                try
                {
                    byte[] bytes = File.ReadAllBytes(filePath);
                    result = ImportFromBytes(bytes);
                }
                catch (IOException ex)
                {
                    result = new ImportResult();
                    result.AddError(ImportConstants.FileErrorRowNumber, ImportMessages.FileReadError + ": " + ex.Message);
                }
            }

            _errorLogger?.LogErrors(filePath, result);
            return result;
        }

        // Импорт файла, загруженного через браузер
        public ImportResult ImportFromStream(Stream stream)
        {
            MemoryStream memory = new MemoryStream();
            stream.CopyTo(memory);

            ImportResult result = ImportFromBytes(memory.ToArray());

            _errorLogger?.LogErrors("загрузка через браузер", result);
            return result;
        }

        public ImportResult ImportFromLines(IEnumerable<string> lines)
        {
            ImportResult result = ImportLines(lines.ToList());

            _errorLogger?.LogErrors("строки", result);
            return result;
        }

        private ImportResult ImportFromBytes(byte[] bytes)
        {
            string encodingName;
            string text = DecodeText(bytes, out encodingName);

            List<string> lines = text.Split('\n').Select(line => line.TrimEnd('\r')).ToList();

            ImportResult result = ImportLines(lines);
            result.EncodingName = encodingName;
            return result;
        }

        // Определяем кодировку файла: сначала пробуем UTF-8,
        // если в файле есть недопустимые для UTF-8 байты - читаем как Windows-1251
        private static string DecodeText(byte[] bytes, out string encodingName)
        {
            // UTF-8 с BOM: первые три байта EF BB BF
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            {
                encodingName = "UTF-8";
                return Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
            }

            try
            {
                // throwOnInvalidBytes: true - выбросит исключение, если это не UTF-8
                UTF8Encoding strictUtf8 = new UTF8Encoding(false, true);
                encodingName = "UTF-8";
                return strictUtf8.GetString(bytes);
            }
            catch (DecoderFallbackException)
            {
                // В .NET кодировку Windows-1251 нужно сначала подключить
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                encodingName = "Windows-1251";
                return Encoding.GetEncoding(1251).GetString(bytes);
            }
        }

        // Основная логика импорта
        private ImportResult ImportLines(List<string> lines)
        {
            ImportResult result = new ImportResult();

            // 1. Ищем заголовок - первую непустую строку
            int headerIndex = lines.FindIndex(line => !string.IsNullOrWhiteSpace(line));
            if (headerIndex == -1)
            {
                result.AddError(ImportConstants.FileErrorRowNumber, ImportMessages.NoData);
                return result;
            }

            // Убираем невидимый символ BOM, если он остался в начале строки
            string headerLine = lines[headerIndex].TrimStart('﻿');

            // 2. Определяем разделитель и проверяем заголовки
            char delimiter = CsvLineParser.DetectDelimiter(headerLine);
            result.Delimiter = delimiter;

            List<string> headers = CsvLineParser.Parse(headerLine, delimiter);
            Dictionary<string, int> columns = GetColumnIndexes(headers);

            foreach (string requiredHeader in ImportConstants.RequiredHeaders)
            {
                if (!columns.ContainsKey(requiredHeader))
                {
                    result.AddError(headerIndex + 1, ImportMessages.InvalidHeaders);
                    return result;
                }
            }

            // Последняя обязательная колонка - warranty_months. Пустую колонку
            // model_number в конце строки можно не писать.
            int minFieldCount = columns["warranty_months"] + 1;
            int maxFieldCount = headers.Count;

            // Артикулы, которые уже добавлены из этого файла
            HashSet<string> importedSkus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 3. Обрабатываем строки с данными
            for (int i = headerIndex + 1; i < lines.Count; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                result.TotalRows++;
                int rowNumber = i + 1; // номер строки в файле (нумерация с 1)

                List<string> values = CsvLineParser.Parse(line, delimiter);
                if (values.Count < minFieldCount || values.Count > maxFieldCount)
                {
                    result.AddError(rowNumber, ImportMessages.InvalidFieldCount);
                    continue;
                }

                List<string> rowErrors = new List<string>();
                Product product = ValidateRow(values, columns, rowErrors);

                if (rowErrors.Count > 0)
                {
                    foreach (string error in rowErrors)
                        result.AddError(rowNumber, error);
                    continue;
                }

                // 4. Проверка дубликата: в этом файле или уже в памяти
                if (importedSkus.Contains(product.Sku) || _repository.GetBySku(product.Sku) != null)
                {
                    result.AddError(rowNumber, ImportMessages.DuplicateSku);
                    continue;
                }

                _repository.Add(product);
                importedSkus.Add(product.Sku);
                result.ImportedCount++;
            }

            // Если после заголовка нет ни одной строки
            if (result.TotalRows == 0)
                result.AddError(ImportConstants.FileErrorRowNumber, ImportMessages.NoData);

            return result;
        }

        // Проверяет все поля строки. Ошибки добавляются в список errors.
        // Если ошибок нет - возвращает заполненный товар.
        private Product ValidateRow(List<string> values, Dictionary<string, int> columns, List<string> errors)
        {
            Product product = new Product();

            product.Sku = GetValue(values, columns, "sku");
            if (product.Sku == "")
                errors.Add(ImportMessages.EmptySku);

            product.Name = GetValue(values, columns, "name");
            if (product.Name == "")
                errors.Add(ImportMessages.EmptyName);

            // Цена: допускаем и точку, и запятую как разделитель дробной части
            string priceText = GetValue(values, columns, "price").Replace(',', '.');
            decimal price;
            if (!decimal.TryParse(priceText, NumberStyles.Number, CultureInfo.InvariantCulture, out price))
                errors.Add(ImportMessages.PriceNotNumber);
            else if (price <= 0)
                errors.Add(ImportMessages.PriceNotPositive);
            else
                product.Price = price;

            int stock;
            if (!int.TryParse(GetValue(values, columns, "stock"), out stock))
                errors.Add(ImportMessages.StockNotInteger);
            else if (stock < 0)
                errors.Add(ImportMessages.NegativeStock);
            else
                product.StockQuantity = stock;

            string isActive = GetValue(values, columns, "is_active").ToLower();
            if (isActive == "true" || isActive == "1")
                product.IsActive = true;
            else if (isActive == "false" || isActive == "0")
                product.IsActive = false;
            else
                errors.Add(ImportMessages.InvalidIsActive);

            product.Brand = GetValue(values, columns, "brand");
            if (product.Brand == "")
                errors.Add(ImportMessages.EmptyBrand);

            int warranty;
            if (int.TryParse(GetValue(values, columns, "warranty_months"), out warranty)
                && warranty >= ImportConstants.MinWarrantyMonths
                && warranty <= ImportConstants.MaxWarrantyMonths)
                product.WarrantyMonths = warranty;
            else
                errors.Add(ImportMessages.InvalidWarranty);

            // Необязательные поля
            product.Description = GetValue(values, columns, "description");
            product.ImageUrl = GetValue(values, columns, "image_url");
            product.ModelNumber = GetValue(values, columns, "model_number");

            string currency = GetValue(values, columns, "currency");
            product.Currency = currency == "" ? ImportConstants.DefaultCurrency : currency.ToUpper();

            string category = GetValue(values, columns, "category_name");
            product.CategoryName = category == "" ? ImportConstants.DefaultCategory : category;

            return product;
        }

        // Составляет словарь "название колонки -> номер колонки".
        // Названия приводятся к нижнему регистру и без пробелов.
        private static Dictionary<string, int> GetColumnIndexes(List<string> headers)
        {
            Dictionary<string, int> columns = new Dictionary<string, int>();

            for (int i = 0; i < headers.Count; i++)
            {
                string name = headers[i].Replace(" ", "").ToLower();
                if (name != "" && !columns.ContainsKey(name))
                    columns.Add(name, i);
            }

            return columns;
        }

        // Возвращает значение колонки в строке (или пустую строку, если колонки нет)
        private static string GetValue(List<string> values, Dictionary<string, int> columns, string columnName)
        {
            int index = columns[columnName];
            if (index < values.Count)
                return values[index].Trim();

            return "";
        }
    }
}
