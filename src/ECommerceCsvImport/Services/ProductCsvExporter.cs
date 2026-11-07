using System.Globalization;
using System.Text;
using ECommerceCsvImport.Models;

namespace ECommerceCsvImport.Services
{
    // Экспорт товаров в CSV того же формата, что и при импорте
    public static class ProductCsvExporter
    {
        public static string ToCsv(IEnumerable<Product> products)
        {
            StringBuilder csv = new StringBuilder();
            csv.AppendLine(string.Join(",", ImportConstants.RequiredHeaders));

            foreach (Product p in products)
            {
                string[] fields =
                {
                    p.Sku,
                    p.Name,
                    p.Description,
                    p.Price.ToString(CultureInfo.InvariantCulture),
                    p.Currency,
                    p.CategoryName,
                    p.StockQuantity.ToString(),
                    p.ImageUrl,
                    p.IsActive ? "true" : "false",
                    p.Brand,
                    p.WarrantyMonths.ToString(),
                    p.ModelNumber
                };

                csv.AppendLine(string.Join(",", fields.Select(CsvLineParser.Escape)));
            }

            return csv.ToString();
        }

        // Байты файла в UTF-8 с BOM, чтобы Excel правильно показывал русские буквы
        public static byte[] ToCsvBytes(IEnumerable<Product> products)
        {
            byte[] bom = Encoding.UTF8.GetPreamble();
            byte[] data = Encoding.UTF8.GetBytes(ToCsv(products));
            return bom.Concat(data).ToArray();
        }
    }
}
