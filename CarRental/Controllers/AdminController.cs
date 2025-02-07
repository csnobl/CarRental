using Microsoft.AspNetCore.Mvc;
using CarRental.Data;
using CarRental.Models;
using System;

namespace CarRental.Controllers
{
    [Route("admin")] //HTTP-förfrågningar till /admin kommer att dirigeras hit.
    public class AdminController : Controller
    {
        private readonly IAdmin _userRepository;

        //Konstruktorn tar emot en IAdmin-repository som en parameter och injicerar den i _userRepository-fältet.
        //Detta gör det möjligt för controllern att använda repositoryt för att hämta användardata.
        public AdminController(IAdmin userRepository) 
        {
            _userRepository = userRepository;
        }

        //Den här metoden kollar om den inloggade användarens roll är "Admin". Den hämtar rollen från en cookie som heter UserRole.
        //Om användarens roll är "Admin", returneras true, annars false.
        private bool IsUserAdmin()
        {
            var userRole = Request.Cookies["UserRole"];
            return userRole == "Admin";
        }

        
        
        
        
        [HttpGet] // Hämtar data (används för att visa sidor).
        public IActionResult Index() //När en användare besöker /admin(eller root av admin-sidan) kommer den här metoden att köras.
        {
            if (IsUserAdmin()) //Den kollar om användaren är en administratör genom att kalla på IsUserAdmin-metoden.
            {
                return RedirectToAction("Index", "Booking"); //Om användaren är en admin, omdirigeras de till Booking-controller
            }

            return View(); //Om användaren inte är en admin, renderas admin-inloggningssidan(som troligen är en vy som finns i projektet).
        }

        [HttpPost] // Skickar data (används t.ex. för formulärinlämningar).
        // Den här metoden hanterar inloggning av administratörer när användaren skickar in sitt e-post och lösenord via en form.
        public IActionResult Login(string email, string password)
        {
            // Först försöker den hämta användaren via _userRepository.GetUser(email, password).
            var admin = _userRepository.GetUser(email, password);

            if (admin == null)
            {
                // Om ingen användare hittas (dvs. admin == null), sätts ett felmeddelande (via ViewData).
                // ViewData är en tillfällig lagring av data som kan skickas till vyn.
                // Här sätts ett felmeddelande som kan användas i HTML - sidan för att informera användaren om att inloggningen misslyckades.
                ViewData["ErrorMessage"] = "Invalid email or password.";
                // och användaren omdirigeras tillbaka till inloggningssidan.
                return View("Index");
            }

            else if (admin is Customer)
            {
                ViewData["ErrorMessage"] = "Customers must log in via the main login page.";
                return View("Index");
            }

            else
            {
                // Om användaren är en Admin, sätts cookies för autentisering (AuthToken och UserRole),   
                Response.Cookies.Append("AuthToken", email);
                Response.Cookies.Append("UserRole", "Admin");

                //och användaren omdirigeras till admin-sidan för bokningar. 
                return RedirectToAction("Index", "Booking");
            }
        }
    }
}
