using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace CarRental.Controllers
{
    public class UserController : Controller
    {
        private readonly ICustomer _customerRepository;
        private readonly IAdmin _adminRepository;

        public UserController(ICustomer customerRepository, IAdmin adminRepository)
        {
            _customerRepository = customerRepository;
            _adminRepository = adminRepository;
        }

        private bool IsUserAdmin()
        {
            var userRole = Request.Cookies["UserRole"];
            return userRole == "Admin";
        }

        // GET: UserController
        public ActionResult Index()
        {
            var userRole = Request.Cookies["UserRole"];
            var userEmail = Request.Cookies["AuthToken"];

            if (string.IsNullOrEmpty(userRole) || string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Home");
            }

            ViewBag.IsAdmin = userRole == "Admin";
            ViewBag.IsCustomer = userRole == "Customer";

            if (userRole == "Admin")
            {
                // Om användaren är en Admin, visa alla användare
                var customers = _customerRepository.GetAll();
                var admins = _adminRepository.GetAll();
                var users = customers.Concat<BaseUser>(admins).ToList();
                return View(users);
            }
            else if (userRole == "Customer")
            {
                // Om användaren är en Customer, visa bara den egna användaren
                var customer = _customerRepository.GetAll(u => u.Email == userEmail).FirstOrDefault();
                var admin = _adminRepository.GetAll(u => u.Email == userEmail).FirstOrDefault();
                var user = customer ?? (BaseUser)admin;

                if (user == null)
                {
                    return RedirectToAction("Index", "Home"); // Hantera om ingen användare hittas
                }

                return View(new List<BaseUser> { user });

                
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: UserController/Details/5
        public ActionResult Details(int id)
        {
            var userRole = Request.Cookies["UserRole"];
            var userEmail = Request.Cookies["AuthToken"]; // Identifiera den inloggade användaren

            if (string.IsNullOrEmpty(userRole) || string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Home"); // Skicka till login om ingen är inloggad
            }

            var user = _customerRepository.GetById(id) ?? (BaseUser)_adminRepository.GetById(id);

            if (user == null)
            {
                return NotFound();
            }

            if (userRole == "Admin")
            {
                // Admin får se alla användares detaljer
                return View(user);
            }
            else if (userRole == "Customer" && user.Email == userEmail)
            {
                // Kunder får bara se sina egna detaljer
                return View(user);
            }

            // Om en kund försöker se en annan användares profil -> Tillbaka till startsidan
            return RedirectToAction("Index", "Home");
        }

        // GET: UserController/Create
        public ActionResult Create()
        {
            if (!IsUserAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: UserController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Customer customer)
        {
            if (!IsUserAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var existingUser = _customerRepository.GetAll(u => u.Email == customer.Email).FirstOrDefault();

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "A user with this email already exists.");
                    return View(customer);
                }

                if (ModelState.IsValid)
                {
                    var newUser = new Customer
                    {
                        Email = customer.Email,
                        Password = customer.Password,
                        Name = customer.Name,
                        LastName = customer.LastName
                    };

                    _customerRepository.Add(newUser);   
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View(customer);
            }
        }

        // GET: UserController/Edit/5
        public ActionResult Edit(int id)
        {
            var userRole = Request.Cookies["UserRole"];
            var userEmail = Request.Cookies["AuthToken"];

            if (userRole == "Admin")
            {
                var user = _customerRepository.GetById(id) ?? (BaseUser)_adminRepository.GetById(id);
                if (user == null)
                {
                    return NotFound();
                }

                ViewBag.RoleList = new SelectList(new List<string> { "Customer", "Admin" }, user is Admin ? "Admin" : "Customer");
                return View(user);
            }
            else if (userRole == "Customer")
            {
                var user = _customerRepository.GetById(id) ?? (BaseUser)_adminRepository.GetById(id);

                // Kunder kan bara editera sina egna uppgifter
                if (user == null || user.Email != userEmail)
                {
                    return Unauthorized();
                }

                return View(user);
            }

            return RedirectToAction("Index", "Home");
        }

        // POST: UserController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, string email, string name, string lastName, string role)
        {
            var userRole = Request.Cookies["UserRole"];
            var userEmail = Request.Cookies["AuthToken"];

            if (userRole == "Customer" && email != userEmail)
            {
                return Unauthorized();
            }

            if (userRole == "Admin")
            {
                // Hantera administratörens redigering
                var user = _customerRepository.GetById(id) ?? (BaseUser)_adminRepository.GetById(id);
                if (user == null) return NotFound();

                user.Email = email;
                user.Name = name;
                user.LastName = lastName;

                if (role == "Admin" && user is not Admin)
                {
                    _customerRepository.Delete((Customer)user);
                    var newAdmin = new Admin { Email = email, Name = name, LastName = lastName };
                    _adminRepository.Add(newAdmin);
                }
                else if (role == "Customer" && user is not Customer)
                {
                    _adminRepository.Delete((Admin)user);
                    var newCustomer = new Customer { Email = email, Name = name, LastName = lastName };
                    _customerRepository.Add(newCustomer);
                }
                else
                {
                    if (user is Customer)
                    {
                        _customerRepository.Update((Customer)user);
                    }
                    else if (user is Admin)
                    {
                        _adminRepository.Update((Admin)user);
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            else if (userRole == "Customer")
            {
                // Kunder kan bara redigera sina egna uppgifter
                var user = _customerRepository.GetById(id) ?? (BaseUser)_adminRepository.GetById(id);
                if (user == null || user.Email != userEmail)
                {
                    return Unauthorized();
                }

                user.Email = email;
                user.Name = name;
                user.LastName = lastName;

                if (user is Customer)
                {
                    _customerRepository.Update((Customer)user);
                }
                else if (user is Admin)
                {
                    _adminRepository.Update((Admin)user);
                }

                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: UserController/Delete/5
        public ActionResult Delete(int id)
        {
            var userRole = Request.Cookies["UserRole"];
            var userEmail = Request.Cookies["AuthToken"];

            if (userRole == "Admin")
            {
                var user = _customerRepository.GetById(id) ?? (BaseUser)_adminRepository.GetById(id);
                return user != null ? View(user) : NotFound();
            }
            else if (userRole == "Customer")
            {
                var user = _customerRepository.GetById(id) ?? (BaseUser)_adminRepository.GetById(id);

                // Kunder kan bara ta bort sina egna uppgifter
                if (user == null || user.Email != userEmail)
                {
                    return Unauthorized();
                }

                return View(user);
            }

            return RedirectToAction("Index", "Home");
        }

        // POST: UserController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, string role)
        {
            var userRole = Request.Cookies["UserRole"];
            var userEmail = Request.Cookies["AuthToken"];

            if (userRole == "Admin")
            {
                try
                {
                    if (role == "Admin")
                    {
                        var admin = _adminRepository.GetById(id);
                        if (admin != null) _adminRepository.Delete(admin);
                    }
                    else
                    {
                        var customer = _customerRepository.GetById(id);
                        if (customer != null) _customerRepository.Delete(customer);
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    return View();
                }
            }
            else if (userRole == "Customer")
            {
                var user = _customerRepository.GetById(id) ?? (BaseUser)_adminRepository.GetById(id);

                // Kunder kan bara ta bort sina egna uppgifter
                if (user == null || user.Email != userEmail)
                {
                    return Unauthorized();
                }

                try
                {
                    if (role == "Admin")
                    {
                        var admin = _adminRepository.GetById(id);
                        if (admin != null) _adminRepository.Delete(admin);
                    }
                    else
                    {
                        var customer = _customerRepository.GetById(id);
                        if (customer != null) _customerRepository.Delete(customer);
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    return View();
                }
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
