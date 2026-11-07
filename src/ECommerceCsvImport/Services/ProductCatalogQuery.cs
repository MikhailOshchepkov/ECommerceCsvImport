using ECommerceCsvImport.Models;

namespace ECommerceCsvImport.Services
{
    // Поиск, фильтрация и сортировка товаров для страницы списка
    public static class ProductCatalogQuery
    {
        public static List<Product> Apply(IEnumerable<Product> products, string? search, string? category, string? sort)
        {
            IEnumerable<Product> query = products;

            // Поиск по названию без учета регистра
            if (!string.IsNullOrWhiteSpace(search))
            {
                string text = search.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(text));
            }

            // Фильтр по категории
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.CategoryName == category);
            }

            // Сортировка. Если сортировка не выбрана - товары идут в порядке импорта
            switch (sort)
            {
                case "name":
                    query = query.OrderBy(p => p.Name);
                    break;
                case "price_asc":
                    query = query.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(p => p.Price);
                    break;
                case "stock_asc":
                    query = query.OrderBy(p => p.StockQuantity);
                    break;
                case "stock_desc":
                    query = query.OrderByDescending(p => p.StockQuantity);
                    break;
            }

            return query.ToList();
        }

        // Список всех категорий без повторов (для выпадающего списка)
        public static List<string> GetCategories(IEnumerable<Product> products)
        {
            return products.Select(p => p.CategoryName).Distinct().OrderBy(c => c).ToList();
        }
    }
}
