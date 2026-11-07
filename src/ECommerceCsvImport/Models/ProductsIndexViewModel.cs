namespace ECommerceCsvImport.Models
{
    // Данные для страницы списка товаров
    public class ProductsIndexViewModel
    {
        public List<Product> Products { get; set; } = new List<Product>();
        public List<string> Categories { get; set; } = new List<string>();

        // Сколько всего товаров в памяти (без учета поиска и фильтра)
        public int AllProductsCount { get; set; }

        public string? Search { get; set; }
        public string? Category { get; set; }
        public string? Sort { get; set; }

        public int TotalCount
        {
            get { return Products.Count; }
        }

        // Общая стоимость - сумма цен товаров
        public decimal TotalValue
        {
            get { return Products.Sum(p => p.Price); }
        }

        // Общее количество единиц на складе
        public int TotalStockQuantity
        {
            get { return Products.Sum(p => p.StockQuantity); }
        }

        // Стоимость остатков на складе (цена * количество)
        public decimal TotalStockValue
        {
            get { return Products.Sum(p => p.Price * p.StockQuantity); }
        }
    }
}
