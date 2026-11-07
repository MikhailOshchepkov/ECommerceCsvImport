using ECommerceCsvImport.Models;

namespace ECommerceCsvImport.Services
{
    // Хранение товаров в памяти приложения (в списке List<Product>).
    // Регистрируется как Singleton, поэтому данные не теряются между запросами.
    public class InMemoryProductRepository : IProductRepository
    {
        private readonly List<Product> _products = new List<Product>();

        public int Count
        {
            get { return _products.Count; }
        }

        public void Add(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            _products.Add(product);
        }

        public IReadOnlyList<Product> GetAll()
        {
            // Возвращаем копию списка, чтобы его нельзя было изменить снаружи
            return _products.ToList();
        }

        // Поиск товара по артикулу без учета регистра и пробелов по краям
        public Product? GetBySku(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
                return null;

            string searchSku = sku.Trim();

            foreach (Product product in _products)
            {
                if (string.Equals(product.Sku.Trim(), searchSku, StringComparison.OrdinalIgnoreCase))
                    return product;
            }

            return null;
        }

        public void Clear()
        {
            _products.Clear();
        }
    }
}
