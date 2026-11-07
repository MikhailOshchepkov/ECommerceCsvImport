using System.Text;
using ECommerceCsvImport.Models;

namespace ECommerceCsvImport.Services
{
    // Записывает ошибки импорта в текстовый файл logs/import-errors.log
    public class FileImportErrorLogger : IImportErrorLogger
    {
        private readonly string _logFilePath;

        public FileImportErrorLogger(string logFilePath)
        {
            _logFilePath = logFilePath;
        }

        public void LogErrors(string source, ImportResult result)
        {
            if (result.ErrorCount == 0)
                return;

            StringBuilder text = new StringBuilder();
            text.AppendLine($"[{DateTime.Now:dd.MM.yyyy HH:mm:ss}] Импорт: {source}, " +
                            $"строк: {result.TotalRows}, импортировано: {result.ImportedCount}, ошибок: {result.ErrorCount}");

            foreach (ImportError error in result.Errors)
            {
                text.AppendLine($"    строка {error.RowNumber}: {error.Message}");
            }

            try
            {
                string? folder = Path.GetDirectoryName(_logFilePath);
                if (!string.IsNullOrEmpty(folder))
                    Directory.CreateDirectory(folder);

                File.AppendAllText(_logFilePath, text.ToString(), Encoding.UTF8);
            }
            catch (IOException)
            {
                // Если журнал записать не удалось, импорт все равно должен работать
            }
        }
    }
}
