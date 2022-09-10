namespace ECommerce1.Models
{
    public class Product : AModel
    {
        public string Name { get; set; }
        public DateTime CreationTime { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

        public Category Category { get; set; }
        public Profile User { get; set; }
        public IList<ProductPhoto> ProductPhotos { get; set; }

        public Product()
        {
            ProductPhotos = new List<ProductPhoto>();
        }
    }
}
