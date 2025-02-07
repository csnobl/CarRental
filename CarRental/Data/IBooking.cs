using CarRental.Models;
using System.Linq.Expressions;

namespace CarRental.Data
{
    public interface IBooking
    {
        Booking GetById(int id);
        IEnumerable<Booking> GetAll();
        IEnumerable<Booking> GetAll(Expression<Func<Booking, bool>> filter = null);
        IEnumerable<Booking> GetBookingsByUserId(int userId);
        void Add(Booking booking);
        void Update(Booking booking);
        void Delete(Booking booking);
        IEnumerable<Booking> GetAllWithCarInfo();
        IEnumerable<Booking> GetAllByUserId(int userId);
    }
}
