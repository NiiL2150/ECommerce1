namespace ECommerce1.Models
{
    public class ProductPhoto : AModel
    {
        public string Url { get; set; }
        public Product Product { get; set; }
    }
}
