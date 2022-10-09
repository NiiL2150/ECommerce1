namespace ECommerce1.Models
{
    public class Profile : AModel
    {
        public string AuthId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string LastName { get; set; }
        public City? City { get; set; }
        public string ProfilePictureURL { get; set; }
        public string PreviewProfilePictureURL { get; set; }
        public IList<Product> Products { get; set; }

        public Profile()
        {
            Products = new List<Product>();
        }
    }
}
