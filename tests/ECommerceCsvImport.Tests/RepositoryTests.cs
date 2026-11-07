using ECommerceCsvImport.Models;
using ECommerceCsvImport.Services;

namespace ECommerceCsvImport.Tests
{
    public class RepositoryTests
    {
        private static Product CreateProduct(string sku = "1001")
        {
            return new Product
            {
                Sku = sku,
                Name = "Wi-Fi роутер Дом.ру Wave Plus",
                Price = 3990m,
                Brand = "Дом.ру"
            };
        }

        [Fact]
        public void Add_AddsProduct_GetAllContainsIt()
        {
            var repository = new InMemoryProductRepository();

            repository.Add(CreateProduct());

            var all = repository.GetAll();
            Assert.Single(all);
            Assert.Equal("1001", all[0].Sku);
            Assert.Equal(1, repository.Count);
        }

        [Fact]
        public void GetBySku_ExistingSku_ReturnsProduct()
        {
            var repository = new InMemoryProductRepository();
            var product = CreateProduct();
            repository.Add(product);

            var found = repository.GetBySku("1001");

            Assert.NotNull(found);
            Assert.Same(product, found);
        }

        [Fact]
        public void GetBySku_IgnoresCaseAndSurroundingSpaces()
        {
            var repository = new InMemoryProductRepository();
            repository.Add(CreateProduct("ABC-1001"));

            Assert.NotNull(repository.GetBySku(" ABC-1001 "));
            Assert.NotNull(repository.GetBySku("abc-1001"));
        }

        [Fact]
        public void NewRepository_IsEmpty()
        {
            var repository = new InMemoryProductRepository();

            Assert.Equal(0, repository.Count);
            Assert.Empty(repository.GetAll());
            Assert.Null(repository.GetBySku("1001"));
        }
    }
}
