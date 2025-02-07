using System.ComponentModel.DataAnnotations;

namespace CarRental.Models
{
    public class Booking
    {
        [Key]
        public  int Id { get; set; }
        [Required]
        public DateOnly StartDate { get; set; }
        [Required]
        public DateOnly EndDate { get; set; }
        [Required]
        public int CarId { get; set; }
        public Car? Car { get; set; }
        [Required]
        public int UserId { get; set; }
        public  BaseUser? User { get; set; }

    }
}
