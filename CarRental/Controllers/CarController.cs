using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.Controllers
{
    
    public class CarController : Controller
    {
        private readonly ICar _carRepository;

        public CarController(ICar carRepository)
        {
            _carRepository = carRepository;
        }

        private bool IsUserAdmin()
        {
            var userRole = Request.Cookies["UserRole"];
            return userRole == "Admin";
        }

        // GET: CarController
        public ActionResult Index()
        {
                if (!IsUserAdmin())
                {
                    return RedirectToAction("Index", "Home");
                }

                return View(_carRepository.GetAll());
        }

        // GET: CarController/Details/5
        public ActionResult Details(int id)
        {
                    if (!IsUserAdmin())
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    var car = _carRepository.GetById(id);
            return View(car);
        }

        // GET: CarController/Create
        public ActionResult Create()
        {
            if (!IsUserAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: CarController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Car car)
        {
            if (!IsUserAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                if (ModelState.IsValid)
                {
                    _carRepository.Add(car);
                    
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: CarController/Edit/5
        public ActionResult Edit(int id)
        {
            if (!IsUserAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            return View(_carRepository.GetById(id));
        }

        // POST: CarController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Car car)
        {
            if (!IsUserAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                if (ModelState.IsValid)
                {
                    _carRepository.Update(car);
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: CarController/Delete/5
        public ActionResult Delete(int id)
        {
            if (!IsUserAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: CarController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            if (!IsUserAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
