namespace ECommerce1.Models.ViewModels
{
    public class AddProductViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public IList<string> ProductPhotos { get; set; }
        public string CategoryId { get; set; }

        public AddProductViewModel()
        {
            ProductPhotos = new List<string>();
        }
    }
}
