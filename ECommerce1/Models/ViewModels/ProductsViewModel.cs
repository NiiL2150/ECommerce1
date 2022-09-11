namespace ECommerce1.Models.ViewModels
{
    public class ProductsViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public Category Category { get; set; }
        public int TotalProductCount { get; set; }
        public int OnPageProductCount { get; set; }
        public int TotalPageCount { get; set; }
        public int CurrentPage { get; set; }


        public enum ProductSorting
        {
            OlderFirst = 1,
            NewerFirst,
            CheaperFirst,
            ExpensiveFirst
        }
    }
}
