namespace ECommerce1.Models
{
    public class City : AModel
    {
        public string Name { get; set; }

        public IList<Profile> Profiles { get; set; }

        public City()
        {
            Profiles = new List<Profile>();
        }
    }
}
