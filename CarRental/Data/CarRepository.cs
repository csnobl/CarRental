using CarRental.Models;

namespace CarRental.Data
{
    public class CarRepository : ICar
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public CarRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public void Add(Car car)
        {
            _applicationDbContext.Cars.Add(car);
            _applicationDbContext.SaveChanges();
        }


        public IEnumerable<Car> GetAll()
        {
            return _applicationDbContext.Cars.OrderBy(c => c.Brand);
        }

        public Car GetById(int id)
        {
            return _applicationDbContext.Cars.FirstOrDefault(c => c.Id==id);
        }

        public void Update(Car car)
        {
            _applicationDbContext.Cars.Update(car);
            _applicationDbContext.SaveChanges();
        }
    }
}
