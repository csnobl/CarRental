using Azure;
using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Controllers
{
    public class LoginController : Controller
    {
        private readonly ICustomer _customerRepository;
        private readonly IAdmin _adminRepository;

        public LoginController(ICustomer customerRepository, IAdmin adminRepository)
        {
            _customerRepository = customerRepository;
            _adminRepository = adminRepository;
        }

        [HttpGet]
        public IActionResult Index(bool redirectToBooking = false, int? carId = null)
        {
            ViewData["RedirectToBooking"] = redirectToBooking;
            ViewData["CarId"] = carId;
            return View("LoginRegister", new Customer());
        }

        [HttpPost]
        public async Task<IActionResult> Index(string email, string password, string? name, string? lastname, bool isRegistering = false, bool redirectToBooking = false, int? carId = null)
        {
            {
                if (isRegistering)
                {

                    if (_customerRepository.GetUserByEmail(email) != null)
                    {
                        ModelState.AddModelError("Email", "An account with this email already exists.");
                        ViewData["RedirectToBooking"] = redirectToBooking;
                        ViewData["CarId"] = carId;
                        return View("LoginRegister");
                    }

                    if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(lastname))
                    {
                        var newUser = new Customer
                        {
                            Email = email,
                            Password = password,
                            Name = name,
                            LastName = lastname
                        };

                        _customerRepository.Add(newUser);
                        await _customerRepository.SaveChangesAsync();

                        Response.Cookies.Append("AuthToken", newUser.Email);
                        Response.Cookies.Append("UserRole", "Customer");
                    }
                    else
                    {
                        ModelState.AddModelError("Name", "Please provide a name.");
                        ModelState.AddModelError("LastName", "Please provide a last name.");
                        return View("LoginRegister");
                    }
                }



                else
                {
                    var customer = _customerRepository.GetUserByEmail(email);
                    if (customer != null && customer.Password == password)
                    {
                        Response.Cookies.Append("AuthToken", customer.Email);
                        Response.Cookies.Append("UserRole", "Customer");
                    }
                    else
                    {
                        
                        var admin = _adminRepository.GetUserByEmail(email);
                        if (admin != null)
                        {
                            ViewData["ErrorMessage"] = "Admins måste logga in via /admin/";
                            return View("LoginRegister");
                        }

                        
                        ViewBag.ErrorMessage = "Ogiltigt användarnamn eller lösenord.";
                        ViewData["RedirectToBooking"] = redirectToBooking;
                        ViewData["CarId"] = carId;
                        return View("LoginRegister");
                    }
                }

                
                if (redirectToBooking && carId.HasValue)
                {
                    return RedirectToAction("CreateBooking", "Booking", new { carId = carId.Value });
                }

                return RedirectToAction("Index", "Home");
            }
        }
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken");
            Response.Cookies.Delete("UserRole");
            return RedirectToAction("Index");
        }
    } }

