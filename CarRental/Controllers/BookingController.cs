using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Reflection.Metadata;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CarRental.Controllers
{
    public class BookingController : Controller
    {

        //Här injiceras de repositories som behövs för att hantera bokningar, kunder, administratörer och bilar.
        //Detta följer Dependency Injection(DI)-principen.
        private readonly IBooking _bookingRepository;
        private readonly ICustomer _customerRepository;
        private readonly IAdmin _adminRepository;
        private readonly ICar _carRepository;

        public BookingController(IBooking bookingRepository, ICustomer customerRepository, IAdmin adminRepository, ICar carRepository)
        {
            _bookingRepository = bookingRepository;
            _customerRepository = customerRepository;
            _adminRepository = adminRepository;
            _carRepository = carRepository;
        }
        // GET: BookingController
        public IActionResult Index()
        {
            //Hämtar autentiseringscookies (AuthToken och UserRole).
            var userEmail = Request.Cookies["AuthToken"];
            var userRole = Request.Cookies["UserRole"];

            if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(userRole))
            {
                return RedirectToAction("Index", "Login");
            }

            IEnumerable<Booking> bookings;

            if (userRole == "Admin")
            {
                //hämtar alla bokningar inklusive bilinformation.
                bookings = _bookingRepository.GetAllWithCarInfo();
                //används här för att skicka information till vyn om huruvida den inloggade användaren är en administratör eller inte.
                ViewBag.IsAdmin = true;
            }

            else if (userRole == "Customer")
            {
                
                var user = _customerRepository.GetUserByEmail(userEmail);
                if (user == null)
                {
                    return Unauthorized();
                }
                //Kundspecifika bokningar hämtas istället via _bookingRepository.GetAllByUserId(user.Id);
                bookings = _bookingRepository.GetAllByUserId(user.Id);
                //sätts för att indikera att en kund är inloggad.
                ViewBag.IsAdmin = false;
            }

            //Om rollen är okänd → Skickar tillbaka en Unauthorized().
            // en 401 HTTP-statuskod skickas tillbaka till webbläsaren. Det betyder att ingen vy visas – istället får klienten en felrespons
            else
            {
                return Unauthorized();
            }

            //Skickar bookings (en lista med bokningar) till en vy.
            //Vyn som används är den som har samma namn som metoden (Index.cshtml i Views/Booking/).
            return View(bookings);

        }

        // GET: BookingController/Details/5
        public ActionResult Details(int id)
        {
            var booking = _bookingRepository.GetById(id);

            //Hämtar en bokning baserat på id och laddar in bil- och användardata.

            booking.Car = _carRepository.GetById(booking.CarId);
            booking.User = _customerRepository.GetById(booking.UserId);

            //Returnerar vyn med bokningsinformationen.
            return View(booking);
        }

        //visa formulär
        //Create låter admin välja bil, vilket behövs för att kunna använda nästa metod: CreateBooking.
        // GET: Booking/Create
        public IActionResult Create()
        {
            var userEmail = Request.Cookies["AuthToken"];
            var userRole = Request.Cookies["UserRole"];

            if (string.IsNullOrEmpty(userEmail) || userRole != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }

            //Hämtar alla tillgängliga bilar och skickar dem till vyn via ViewBag.Cars.
            var cars = _carRepository.GetAll().Where(c => c.IsActive).ToList();
            ViewBag.Cars = cars; // Skicka bilarna till vyn

            //skapar ett tomt objekt av Booking-klassen för att förbereda ett tomt formulär för användaren.
            //Modellbindning: När användaren skickar in formuläret kommer de ifyllda värdena från formuläret att
            //bindas till en ny Booking-instans i controller, och du kan vidare bearbeta bokningen där.
            return View(new Booking());
        }

        //spara bokning
        // POST: Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Booking booking)
        {
            var userEmail = Request.Cookies["AuthToken"];
            var userRole = Request.Cookies["UserRole"];

            if (string.IsNullOrEmpty(userEmail) || userRole != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }

            var admin = _adminRepository.GetUserByEmail(userEmail);
            if (admin == null)
            {
                return Unauthorized();
            }

            //booking.StartDate:startdatumet för den bokning som användaren försöker skapa.
            //DateOnly.FromDateTime(DateTime.Today): Här konverterar du det aktuella datumet (utan tid) till en DateOnly-instans.
            //DateTime.Today returnerar dagens datum med tidstämpeln satt till midnatt (00:00:00),
            if (booking.StartDate < DateOnly.FromDateTime(DateTime.Today))
            {
                //ModelState är ett objekt som lagrar all information om formulärets validering. Det håller reda på om fälten i formuläret är giltiga eller inte.
                //AddModelError() används för att lägga till ett felmeddelande i ModelState.
                //Detta gör att ett fel visas för användaren när de försöker skicka in ett ogiltigt formulär.
                //I detta fall tillhandahålls ett tomt fält som första parameter (""), vilket innebär att felmeddelandet inte är kopplat till ett specifikt formulärfält
                ModelState.AddModelError("", "You can only book future dates.");
            }

            if (!ModelState.IsValid)
            {
                //ViewBag används här endast för att skicka en lista med bilar (ViewBag.Cars) och inte för att skicka felmeddelanden.
                ViewBag.Cars = _carRepository.GetAll().Where(c => c.IsActive).ToList();
                return View(booking);
            }

            //Om allt är giltigt (alla fält är ifyllda korrekt och det finns inga fel), sätts booking.UserId till adminens Id
            booking.UserId = admin.Id;
            //Därefter läggs bokningen till i bokningsrepositoryt
            _bookingRepository.Add(booking);

            //användaren till Index-sidan för att visa en lista på alla bokningar.
            return RedirectToAction("Index");
        }

        //GET: CreateBooking (visa formulär med vald bil)
        //CreateBooking används när admin klickar "Boka" på en bil och vill skapa bokningen direkt
        // GET: BookingController/CreateBooking
        public ActionResult CreateBooking(int carId)
        {
            var userEmail = Request.Cookies["AuthToken"];
            var userRole = Request.Cookies["UserRole"];

            if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(userRole))
            {
                //Temporärt sparar carId i TempData.
                //Detta görs för att kunna använda det efter inloggning.
                TempData["CarIdToBook"] = carId;

                //Skickar användaren till LoginController → Index().
                //Skickar också med en parameter redirectToBooking = true och bilens ID(carId).
                return RedirectToAction("Index", "Login", new { redirectToBooking = true, carId = carId });
            }

            if (userRole == "Customer")
            {
                //TempData används här för att skicka ett felmeddelande till nästa request efter en redirect.
                TempData["ErrorMessage"] = "Customers are only allowed to book via the 'Cars to Rent'-page.";
                return RedirectToAction("Index", "Home");
            }

            var car = _carRepository.GetById(carId);
            if (car == null || !car.IsActive)
            {
                TempData["ErrorMessage"] = "This car is not available.";
                return RedirectToAction("Index", "Home");
            }

            BaseUser user = null;

            if (userRole == "Admin")
            {
                user = _adminRepository.GetUserByEmail(userEmail);
            }

            if (user == null)
            {
                return Unauthorized();
            }

            //Den här koden används för att hämta alla bokade datum för en viss bil (carId) och skapa en lista över dessa datum i strängformat (yyyy-MM-dd).
            //_bookingRepository.GetAll(b => b.CarId == carId). Detta hämtar alla bokningar i databasen där bilens CarId matchar carId
            //Enumerable.Range(0, antal dagar). Skapar en sekvens av heltal från 0 upp till(EndDate -StartDate), vilket motsvarar antalet dagar i bokningen.
            //Select(offset => b.StartDate.AddDays(offset)). För varje dag i bokningen adderas offset till StartDate.
            //SelectMany(). Eftersom varje bokning kan ha flera datum, returnerar.SelectMany() en platt lista med alla bokade datum iställe
            //för en lista med listor.
            //Select(date => date.ToString("yyyy-MM-dd")). Konverterar varje DateOnly - datum till en sträng i formatet yyyy-MM-dd.
            //ToList(); Returnerar den färdiga listan med alla bokade datum som strängar.
            var bookedDates = _bookingRepository.GetAll(b => b.CarId == carId)
                                        .SelectMany(b => Enumerable.Range(0, b.EndDate.DayNumber - b.StartDate.DayNumber + 1)
                                                                   .Select(offset => b.StartDate.AddDays(offset)))
                                        .Select(date => date.ToString("yyyy-MM-dd"))
                                        .ToList();

            //Om allt är korrekt → Returnera en bokningsvy
            var booking = new Booking { CarId = carId, UserId = user.Id };
            return View(booking);
        }

        // bokningen sparas och användaren skickas till bekräftelsesidan.
        // POST: BookingController/CreateBooking
        [HttpPost]
        //Skyddar mot CSRF-attacker genom att kräva en giltig anti-forgery-token i formuläret.
        [ValidateAntiForgeryToken]
        public ActionResult CreateBooking(Booking booking)
        {
            var userEmail = Request.Cookies["AuthToken"];
            var userRole = Request.Cookies["UserRole"];

            if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(userRole))
            {
                TempData["CarIdToBook"] = booking.CarId;
                return RedirectToAction("Index", "Login", new { redirectToBooking = true, carId = booking.CarId });
            }

            if (userRole == "Customer")
            {
                TempData["ErrorMessage"] = "Customers are only allowed to book via the 'Cars to Rent'-page.";
                return RedirectToAction("Index", "Home");
            }

            try
            {

            var car = _carRepository.GetById(booking.CarId);
            if (car == null || !car.IsActive)
            {
                TempData["ErrorMessage"] = "This car is not available.";
                return RedirectToAction("Index", "Home");
            }

            BaseUser user = null;

            if (userRole == "Admin")
            {
                user = _adminRepository.GetUserByEmail(userEmail);
            }

            if (user == null)
            {
                return Unauthorized();
            }

                if (booking.StartDate < DateOnly.FromDateTime(DateTime.Today))
                {
                    ModelState.AddModelError("", "You can only book future dates.");

                    //Returnerar CreateBooking-vyn med de inmatade uppgifterna.
                    return View(booking);
                }

                if (ModelState.IsValid)
                {
                        //Efter att bokningen har lagts till (_bookingRepository.Add(booking);), hämtar vi Id från det nyss skapade booking-objektet.
                        //Sedan skickar vi användaren vidare till BookingConfirmation, där vi vill visa bokningsdetaljerna för den specifika bokningen.
                        //id = booking.Id används för att hämta rätt bokning i BookingConfirmation(int id).
                        _bookingRepository.Add(booking);
                        return RedirectToAction("BookingConfirmation", new { id = booking.Id });
                    }

                ModelState.AddModelError("", "Det finns ett valideringsfel.");
                return View(booking);
                
            }
            catch
            {
                return View(booking);
            }
        }

        // GET: BookingController/BookingConfirmation/5
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

        // GET: BookingController/Edit/5
        public ActionResult Edit(int id)
        {
            var booking = _bookingRepository.GetById(id);

            booking.Car = _carRepository.GetById(booking.CarId);
            booking.User = _customerRepository.GetById(booking.UserId);

            return View(booking);
        }

        // POST: BookingController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Booking booking)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _bookingRepository.Update(booking);
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: BookingController/Delete/5
        public ActionResult Delete(int id)
        {
            var booking = _bookingRepository.GetById(id);
            if (booking == null)
            {
                return NotFound();
            }
            return View(booking);
        }

        // POST: BookingController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                var booking = _bookingRepository.GetById(id);
                if (booking == null)
                {
                    return NotFound();
                }

                if (booking.StartDate < DateOnly.FromDateTime(DateTime.Now))
                {
                    ModelState.AddModelError("", "You can only cancel future bookings!");
                    return View(booking);
                }

                _bookingRepository.Delete(booking);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while deleting the booking: " + ex.Message);
                return View();
            }
        }
    }
}
