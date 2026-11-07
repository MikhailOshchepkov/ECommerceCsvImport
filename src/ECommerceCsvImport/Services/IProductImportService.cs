using ECommerceCsvImport.Models;

namespace ECommerceCsvImport.Services
{
    // Интерфейс сервиса импорта товаров из CSV
    public interface IProductImportService
    {
        // Импорт из файла на диске
        ImportResult ImportFromCsv(string filePath);

        // Импорт из файла, загруженного через браузер
        ImportResult ImportFromStream(Stream stream);

        // Импорт из набора строк (удобно для unit-тестов)
        ImportResult ImportFromLines(IEnumerable<string> lines);
    }
}
