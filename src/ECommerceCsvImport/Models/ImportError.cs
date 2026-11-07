namespace ECommerceCsvImport.Models
{
    // Ошибка импорта: номер строки файла и описание проблемы
    public class ImportError
    {
        public int RowNumber { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
