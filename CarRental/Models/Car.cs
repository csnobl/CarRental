namespace CarRental.Models
{
    public class Car
    {
        public int Id { get; set; }
        public int Vintage { get; set; }
        public string Brand { get; set; }
        public int Price { get; set; }
        public string ImgUrl { get; set; }
        public bool IsActive { get; set; }

    }
}
