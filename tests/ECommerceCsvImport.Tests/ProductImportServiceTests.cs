using ECommerceCsvImport.Models;
using ECommerceCsvImport.Services;

namespace ECommerceCsvImport.Tests
{
    /// <summary>
    /// Тесты 1–23 из плана тестирования сервиса импорта.
    /// </summary>
    public class ProductImportServiceTests
    {
        private const string Header =
            "sku,name,description,price,currency,category_name,stock,image_url,is_active,brand,warranty_months,model_number";

        private const string WavePlusRow =
            "1001,Wi-Fi роутер Дом.ру Wave Plus,Роутер Wi-Fi 6 AX3000 для квартиры,3990,RUB,Роутеры,25,/images/wave-plus.jpg,true,Дом.ру,12,Wave Plus AX3000";

        private const string MovixGoRow =
            "1002,ТВ-приставка Movix Go,Смарт-приставка Android TV в формате донгла,4990,RUB,ТВ-приставки,30,/images/movix-go.jpg,true,Movix,12,Movix Go";

        private const string DuplicateSkuRow =
            "1001,Wi-Fi роутер TP-Link Archer AX23,Другой роутер,5490,RUB,Роутеры,10,/images/archer-ax23.jpg,true,TP-Link,36,Archer AX23";

        private readonly InMemoryProductRepository _repository = new();
        private readonly ProductImportService _service;

        public ProductImportServiceTests()
        {
            _service = new ProductImportService(_repository);
        }

        private ImportResult Import(params string[] lines)
        {
            return _service.ImportFromLines(lines);
        }

        private static string CreateTempFile(string content)
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");
            File.WriteAllText(path, content);
            return path;
        }

        // Тест 1
        [Fact]
        public void ImportFromLines_OneValidRow_ImportsProduct()
        {
            var result = Import(Header, WavePlusRow);

            Assert.Equal(1, result.TotalRows);
            Assert.Equal(1, result.ImportedCount);
            Assert.Empty(result.Errors);

            var product = _repository.GetBySku("1001");
            Assert.NotNull(product);
            Assert.Equal("Wi-Fi роутер Дом.ру Wave Plus", product!.Name);
            Assert.Equal(3990m, product.Price);
            Assert.Equal("Дом.ру", product.Brand);
            Assert.Equal(12, product.WarrantyMonths);
            Assert.Equal("Wave Plus AX3000", product.ModelNumber);
        }

        // Тест 2
        [Fact]
        public void ImportFromLines_SeveralValidRows_ImportsAll()
        {
            var result = Import(Header, WavePlusRow, MovixGoRow);

            Assert.Equal(2, result.TotalRows);
            Assert.Equal(2, result.ImportedCount);
            Assert.Empty(result.Errors);
            Assert.Equal(2, _repository.Count);
        }

        // Тест 3
        [Fact]
        public void ImportFromCsv_FileNotFound_ReturnsError()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");

            var result = _service.ImportFromCsv(path);

            Assert.Equal(0, result.TotalRows);
            Assert.Equal(0, result.ImportedCount);
            Assert.Single(result.Errors);
            Assert.Contains("Файл не найден", result.Errors[0].Message);
        }

        // Тест 4
        [Fact]
        public void ImportFromCsv_EmptyFile_ReturnsNoDataError()
        {
            var path = CreateTempFile(string.Empty);

            try
            {
                var result = _service.ImportFromCsv(path);

                Assert.Equal(0, result.TotalRows);
                Assert.Equal(0, result.ImportedCount);
                Assert.Single(result.Errors);
                Assert.Contains("Данные для импорта отсутствуют", result.Errors[0].Message);
            }
            finally
            {
                File.Delete(path);
            }
        }

        // Тест 5
        [Fact]
        public void ImportFromLines_HeaderOnly_ReturnsNoDataError()
        {
            var result = Import(Header);

            Assert.Equal(0, result.TotalRows);
            Assert.Equal(0, result.ImportedCount);
            Assert.Single(result.Errors);
            Assert.Contains("Данные для импорта отсутствуют", result.Errors[0].Message);
        }

        // Тест 6
        [Fact]
        public void ImportFromLines_InvalidHeader_ReturnsHeaderError()
        {
            var result = Import("sku,name,description,price,currency,category_name,stock,image_url,is_active");

            Assert.Equal(0, result.TotalRows);
            Assert.Equal(0, result.ImportedCount);
            Assert.NotEmpty(result.Errors);
            Assert.Contains("Неверные заголовки CSV", result.Errors[0].Message);
            Assert.Equal(1, result.Errors[0].RowNumber);
        }

        // Тест 7
        [Fact]
        public void ImportFromLines_WrongFieldCount_ReturnsError()
        {
            var result = Import(Header, "1001,Wi-Fi роутер Дом.ру Wave Plus");

            Assert.Equal(1, result.TotalRows);
            Assert.Equal(0, result.ImportedCount);
            Assert.NotEmpty(result.Errors);
            Assert.Contains("Неверное количество полей в строке", result.Errors[0].Message);
        }

        // Тест 8
        [Fact]
        public void ImportFromLines_EmptySku_ReturnsError()
        {
            var result = Import(Header,
                ",Wi-Fi роутер Дом.ру Wave Plus,Роутер Wi-Fi 6,3990,RUB,Роутеры,25,/images/wave-plus.jpg,true,Дом.ру,12,Wave Plus AX3000");

            Assert.Equal(0, result.ImportedCount);
            Assert.Contains(result.Errors, e => e.Message.Contains("Поле sku не может быть пустым"));
        }

        // Тест 9
        [Fact]
        public void ImportFromLines_EmptyName_ReturnsError()
        {
            var result = Import(Header,
                "1001,,Роутер Wi-Fi 6,3990,RUB,Роутеры,25,/images/wave-plus.jpg,true,Дом.ру,12,Wave Plus AX3000");

            Assert.Equal(0, result.ImportedCount);
            Assert.Contains(result.Errors, e => e.Message.Contains("Поле name не может быть пустым"));
        }

        // Тест 10
        [Fact]
        public void ImportFromLines_NegativePrice_ReturnsError()
        {
            var result = Import(Header,
                "1003,ТВ-приставка Movix Pro Voice,Приставка 4K с голосовым поиском,-500,RUB,ТВ-приставки,12,/images/movix-pro-voice.jpg,true,Movix,12,Movix Pro Voice");

            Assert.Equal(0, result.ImportedCount);
            Assert.Contains(result.Errors, e => e.Message.Contains("Цена должна быть больше нуля"));
            Assert.Equal(2, result.Errors[0].RowNumber);
        }

        // Тест 11
        [Fact]
        public void ImportFromLines_NonNumericPrice_ReturnsError()
        {
            var result = Import(Header,
                "1003,ТВ-приставка Movix Pro Voice,Приставка 4K с голосовым поиском,abc,RUB,ТВ-приставки,12,/images/movix-pro-voice.jpg,true,Movix,12,Movix Pro Voice");

            Assert.Equal(0, result.ImportedCount);
            Assert.Contains(result.Errors, e => e.Message.Contains("Цена должна быть числом"));
        }

        // Тест 12
        [Fact]
        public void ImportFromLines_NegativeStock_ReturnsError()
        {
            var result = Import(Header,
                "1004,Домашняя камера Omny base Luna 2,Wi-Fi камера Full HD,3490,RUB,Видеонаблюдение,-5,/images/luna2.jpg,true,Omny,12,Luna 2");

            Assert.Equal(0, result.ImportedCount);
            Assert.Contains(result.Errors, e => e.Message.Contains("Остаток на складе не может быть отрицательным"));
        }

        // Тест 13
        [Fact]
        public void ImportFromLines_InvalidIsActive_ReturnsError()
        {
            var result = Import(Header,
                "1007,Wi-Fi роутер TP-Link Archer AX23,Двухдиапазонный роутер Wi-Fi 6,5490,RUB,Роутеры,10,/images/archer-ax23.jpg,yes,TP-Link,36,Archer AX23");

            Assert.Equal(0, result.ImportedCount);
            Assert.Contains(result.Errors, e => e.Message.Contains("Поле is_active должно иметь значение true, false, 1 или 0"));
        }

        // Тест 14
        [Fact]
        public void ImportFromLines_MissingBrand_ReturnsError()
        {
            var result = Import(Header,
                "1005,Уличная IP-камера Omny base miniDome2E,,7990,RUB,Видеонаблюдение,6,/images/minidome.jpg,true,,24,miniDome2E v1.1");

            Assert.Equal(0, result.ImportedCount);
            Assert.Contains(result.Errors, e => e.Message.Contains("Поле brand обязательно для варианта «Электроника»"));
        }

        // Тест 15
        [Fact]
        public void ImportFromLines_NegativeWarranty_ReturnsError()
        {
            var result = Import(Header,
                "1006,Mesh-система TP-Link Deco X20 (2 шт.),Бесшовная сеть Wi-Fi 6,12990,RUB,Роутеры,5,/images/deco-x20.jpg,true,TP-Link,-1,Deco X20-2");

            Assert.Equal(0, result.ImportedCount);
            Assert.Contains(result.Errors, e => e.Message.Contains("Гарантия должна быть целым числом от 0 до 60 месяцев"));
        }

        // Тест 16
        [Fact]
        public void ImportFromLines_WarrantyAbove60_ReturnsError()
        {
            var result = Import(Header,
                "1006,Mesh-система TP-Link Deco X20 (2 шт.),Бесшовная сеть Wi-Fi 6,12990,RUB,Роутеры,5,/images/deco-x20.jpg,true,TP-Link,61,Deco X20-2");

            Assert.Equal(0, result.ImportedCount);
            Assert.Contains(result.Errors, e => e.Message.Contains("Гарантия должна быть целым числом от 0 до 60 месяцев"));
        }

        // Тест 17
        [Fact]
        public void ImportFromLines_DuplicateSkuInSameFile_SkipsSecond()
        {
            var result = Import(Header, WavePlusRow, DuplicateSkuRow);

            Assert.Equal(2, result.TotalRows);
            Assert.Equal(1, result.ImportedCount);
            Assert.Single(result.Errors);
            Assert.Contains("Артикул товара уже существует", result.Errors[0].Message);
            Assert.Equal(3, result.Errors[0].RowNumber);

            var product = _repository.GetBySku("1001");
            Assert.NotNull(product);
            Assert.Equal("Wi-Fi роутер Дом.ру Wave Plus", product!.Name);
        }

        // Тест 18
        [Fact]
        public void ImportFromLines_DuplicateSkuInRepository_IsNotAdded()
        {
            _repository.Add(new Product { Sku = "1001", Name = "Wi-Fi роутер Дом.ру Wave Plus", Price = 3990m, Brand = "Дом.ру" });

            var result = Import(Header, DuplicateSkuRow);

            Assert.Equal(0, result.ImportedCount);
            Assert.Equal(1, _repository.Count);
            Assert.Contains(result.Errors, e => e.Message.Contains("Артикул товара уже существует"));
            Assert.Equal("Wi-Fi роутер Дом.ру Wave Plus", _repository.GetBySku("1001")!.Name);
        }

        // Тест 19
        [Fact]
        public void ImportFromLines_SeveralErrorsInOneRow_ReportsAll()
        {
            var result = Import(Header, ",,Некорректная строка,-10,RUB,Роутеры,-5,/images/x.jpg,yes,,61");

            Assert.Equal(0, result.ImportedCount);
            Assert.True(result.Errors.Count >= 4);
            Assert.Equal(0, _repository.Count);
            Assert.All(result.Errors, e => Assert.Equal(2, e.RowNumber));
        }

        // Тест 20
        [Fact]
        public void ImportFromLines_EmptyOptionalFields_AreAllowed()
        {
            var result = Import(Header, "1011,Кабель HDMI 2.0 1.5 м,,490,RUB,Аксессуары,50,,true,Cablexpert,12,CC-HDMI4-1.5M");

            Assert.Equal(1, result.ImportedCount);
            Assert.Empty(result.Errors);

            var product = _repository.GetBySku("1011")!;
            Assert.Equal(string.Empty, product.Description);
            Assert.Equal(string.Empty, product.ImageUrl);
        }

        // Тест 21
        [Fact]
        public void ImportFromLines_EmptyCurrency_DefaultsToRub()
        {
            var result = Import(Header, "1011,Кабель HDMI 2.0 1.5 м,,490,,Аксессуары,50,,true,Cablexpert,12,CC-HDMI4-1.5M");

            Assert.Equal(1, result.ImportedCount);
            Assert.Equal("RUB", _repository.GetBySku("1011")!.Currency);
        }

        // Тест 22
        [Fact]
        public void ImportFromLines_EmptyCategory_DefaultsToNoCategory()
        {
            var result = Import(Header, "1011,Кабель HDMI 2.0 1.5 м,,490,RUB,,50,,true,Cablexpert,12,CC-HDMI4-1.5M");

            Assert.Equal(1, result.ImportedCount);
            Assert.Equal("Без категории", _repository.GetBySku("1011")!.CategoryName);
        }

        // Тест 23
        [Fact]
        public void ImportFromLines_CyrillicIsPreserved()
        {
            Import(Header, WavePlusRow);

            var product = _repository.GetBySku("1001")!;
            Assert.Equal("Wi-Fi роутер Дом.ру Wave Plus", product.Name);
            Assert.Equal("Роутер Wi-Fi 6 AX3000 для квартиры", product.Description);
            Assert.Equal("Роутеры", product.CategoryName);
        }
    }
}
