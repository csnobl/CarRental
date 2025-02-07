using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.Controllers
{
    public class CustomerCarController : Controller
    {
        private readonly ICar _carRepository;
        private readonly IBooking _bookingRepository;
        private readonly ICustomer _customerRepository;
        private readonly IAdmin _adminRepository;

        public CustomerCarController(ICar carRepository, IBooking bookingRepository, ICustomer customerRepository, IAdmin adminRepository)
        {
            _carRepository = carRepository;
            _bookingRepository = bookingRepository;
            _customerRepository = customerRepository;
            _adminRepository = adminRepository;
        }

        // GET: CustomerCarController
        public ActionResult Index()
        {
            return View(_carRepository.GetAll().Where(c => c.IsActive).ToList());
        }

        // GET: CustomerCarController/Book/5
        public ActionResult Book(int id)
        {
            var userEmail = Request.Cookies["AuthToken"];
            var userRole = Request.Cookies["UserRole"];

            if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(userRole))
            {
                return RedirectToAction("Index", "Login");
            }

            BaseUser user = null;
            if (userRole == "Customer")
            {
                user = _customerRepository.GetUserByEmail(userEmail);
            }
            else if (userRole == "Admin")
            {
                user = _adminRepository.GetUserByEmail(userEmail);
            }

            if (user == null)
            {
                return Unauthorized();
            }

            var car = _carRepository.GetById(id);
            if (car == null || !car.IsActive)
            {
                TempData["ErrorMessage"] = "This car is not available.";
                return RedirectToAction("Index", "Home");
            }

            var bookedDates = _bookingRepository.GetAll(b => b.CarId == id)
               .SelectMany(b => Enumerable.Range(0, b.EndDate.DayNumber - b.StartDate.DayNumber + 1)
                                          .Select(offset => b.StartDate.AddDays(offset)))
               .Select(date => date.ToString("yyyy-MM-dd"))
               .ToList();

            var model = new Booking
            {
                CarId = id,
                UserId = user.Id,
            };

            ViewBag.BookedDates = bookedDates;

            return View(model);
        }
        
        // POST: CustomerCarController/Book
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Book(Booking booking)
        {
            try
            {
                Console.WriteLine($"Försöker skapa en bokning för bil {booking.CarId} med startdatum {booking.StartDate} och slutdatum {booking.EndDate}");

                if (booking.StartDate < DateOnly.FromDateTime(DateTime.Today))
                {
                    ModelState.AddModelError("", "You can only book future dates.");
                }

                if (ModelState.IsValid)
                {
                    _bookingRepository.Add(booking);
                    Console.WriteLine("Bokningen har lagts till i databasen.");
                }
                return RedirectToAction("BookingConfirmation", new { id = booking.Id });

                ModelState.AddModelError("", "Det finns ett valideringsfel.");
                return View(booking);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ett fel inträffade: {ex.Message}");
                return View(booking);
            }
        }

        // GET: CustomerCarController/BookingConfirmation/5
        public ActionResult BookingConfirmation(int id)
        {
            var booking = _bookingRepository.GetById(id);
            if (booking == null)
            {
                return NotFound();
            }


            var car = _carRepository.GetById(booking.CarId);
            ViewBag.CarBrand = car != null ? car.Brand : "Unknown Brand";

            return View(booking);
        }
    }
}
