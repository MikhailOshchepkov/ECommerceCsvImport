namespace ECommerceCsvImport.Models
{
    // Данные для страницы импорта
    public class ImportViewModel
    {
        public string FilePath { get; set; } = string.Empty;

        // Откуда был импорт: путь к файлу или имя загруженного файла
        public string SourceName { get; set; } = string.Empty;

        public ImportResult? Result { get; set; }

        // Примеры CSV-файлов из папки wwwroot/csv
        public List<string> SampleFiles { get; set; } = new List<string>();
    }
}
