namespace ECommerceCsvImport.Models
{
    // Тексты сообщений об ошибках импорта
    public static class ImportMessages
    {
        public const string FileNotFound = "Файл не найден";
        public const string NoData = "Данные для импорта отсутствуют";
        public const string InvalidHeaders = "Неверные заголовки CSV";
        public const string InvalidFieldCount = "Неверное количество полей в строке";
        public const string DuplicateSku = "Артикул товара уже существует";

        public const string EmptySku = "Поле sku не может быть пустым";
        public const string EmptyName = "Поле name не может быть пустым";
        public const string PriceNotNumber = "Цена должна быть числом";
        public const string PriceNotPositive = "Цена должна быть больше нуля";
        public const string StockNotInteger = "Остаток на складе должен быть целым числом";
        public const string NegativeStock = "Остаток на складе не может быть отрицательным";
        public const string InvalidIsActive = "Поле is_active должно иметь значение true, false, 1 или 0";
        public const string EmptyBrand = "Поле brand обязательно для варианта «Электроника»";
        public const string InvalidWarranty = "Гарантия должна быть целым числом от 0 до 60 месяцев";

        public const string FilePathRequired = "Укажите путь к CSV-файлу";
        public const string FileNotSelected = "Выберите CSV-файл для загрузки";
        public const string FileReadError = "Не удалось прочитать файл";
        public const string WrongExtension = "Допускаются только файлы с расширением .csv";
        public const string FileTooLarge = "Файл слишком большой (максимум 5 МБ)";
        public const string UnexpectedError = "Ошибка при импорте";
    }
}
