using CarRental.Models;

namespace CarRental.Data
{
    public interface ICar
    {
        Car GetById(int id);
        IEnumerable<Car> GetAll();
        void Add(Car car);
        void Update(Car car);
        
    }
}
