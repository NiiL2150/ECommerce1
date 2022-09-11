namespace ECommerce1.Models.ViewModels
{
    public class RegistrationCredentials
    {
        private string _email;
        public string Email
        {
            get { return _email; }
            set
            {
                _email = value.ToLower();
            }
        }
        public string Password { get; set; }
        public string PasswordConfirmation { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string? CityId { get; set; }
    }
}
