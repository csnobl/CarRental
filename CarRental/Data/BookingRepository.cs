using CarRental.Models;
using CarRental.Data;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Data
{
    public class BookingRepository : IBooking
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public BookingRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public void Add(Booking booking)
        {
            try
            {
                Console.WriteLine($"Booking ID before save: {booking.Id}");

                booking.Id = 0;

                _applicationDbContext.Bookings.Add(booking);
                _applicationDbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ett fel inträffade: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inre fel: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public void Delete(Booking booking)
        {
            _applicationDbContext.Bookings.Remove(booking);
            _applicationDbContext.SaveChanges();
        }

        public IEnumerable<Booking> GetAll()
        {
            return _applicationDbContext.Bookings.OrderBy(b => b.Id).ToList();
        }

        public IEnumerable<Booking> GetAll(Expression<Func<Booking, bool>> filter)
        {
            var query = _applicationDbContext.Bookings.AsQueryable();
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return query.OrderBy(b => b.Id).ToList();
        }

        public IEnumerable<Booking> GetAllByUserId(int userId)
        {
            return _applicationDbContext.Bookings
            .Where(b => b.UserId == userId)
            .Include(b => b.Car)
            .ToList();
        }

        public IEnumerable<Booking> GetAllWithCarInfo()
        {
            return _applicationDbContext.Bookings.Include(b => b.Car).ToList();
        }

        public IEnumerable<Booking> GetBookingsByUserId(int userId)
        {
            return _applicationDbContext.Bookings
                .Where(b => b.UserId == userId)
                .OrderBy(b => b.Id)
                .ToList();
        }

        public Booking GetById(int id)
        {
            return _applicationDbContext.Bookings.FirstOrDefault(b => b.Id==id);
        }

        public void Update(Booking booking)
        {
            _applicationDbContext.Bookings.Update(booking);
            _applicationDbContext.SaveChanges();
        }
    }
}
