namespace ECommerceCsvImport.Models
{
    // Результат импорта CSV-файла
    public class ImportResult
    {
        // Сколько строк с данными было обработано (без заголовка и пустых строк)
        public int TotalRows { get; set; }

        // Сколько товаров успешно добавлено
        public int ImportedCount { get; set; }

        public List<ImportError> Errors { get; set; } = new List<ImportError>();

        public int ErrorCount
        {
            get { return Errors.Count; }
        }

        // Разделитель и кодировка, с которыми был прочитан файл (для вывода на странице)
        public char Delimiter { get; set; } = ',';
        public string EncodingName { get; set; } = string.Empty;

        public void AddError(int rowNumber, string message)
        {
            Errors.Add(new ImportError { RowNumber = rowNumber, Message = message });
        }
    }
}
