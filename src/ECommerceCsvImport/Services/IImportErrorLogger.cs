using ECommerceCsvImport.Models;

namespace ECommerceCsvImport.Services
{
    // Интерфейс журнала ошибок импорта
    public interface IImportErrorLogger
    {
        void LogErrors(string source, ImportResult result);
    }
}
