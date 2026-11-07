namespace ECommerceCsvImport.Models
{
    // Константы модуля импорта, чтобы не было "магических" чисел и строк в коде
    public static class ImportConstants
    {
        public const string DefaultCurrency = "RUB";
        public const string DefaultCategory = "Без категории";

        public const int MinWarrantyMonths = 0;
        public const int MaxWarrantyMonths = 60;

        // Номер строки для ошибок, которые относятся ко всему файлу
        public const int FileErrorRowNumber = 0;

        // Максимальный размер загружаемого файла - 5 МБ
        public const long MaxUploadFileSize = 5 * 1024 * 1024;

        // Заголовки, которые обязательно должны быть в CSV-файле
        public static readonly string[] RequiredHeaders =
        {
            "sku", "name", "description", "price", "currency", "category_name",
            "stock", "image_url", "is_active", "brand", "warranty_months", "model_number"
        };
    }
}
