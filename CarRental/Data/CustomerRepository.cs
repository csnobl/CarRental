using CarRental.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CarRental.Data
{
    public class CustomerRepository : ICustomer
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public CustomerRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public void Add(BaseUser user)
        {
            _applicationDbContext.Customers.Add((Customer)user);
            _applicationDbContext.SaveChanges();
        }

        public void Delete(BaseUser user)
        {
            _applicationDbContext.Customers.Remove((Customer)user);
            _applicationDbContext.SaveChanges();
        }

        public IEnumerable<BaseUser> GetAll()
        {
            return _applicationDbContext.Customers.OrderBy(u => u.LastName);
        }

        public IEnumerable<BaseUser> GetAll(Expression<Func<BaseUser, bool>> filter)
        {
            var query = _applicationDbContext.Customers.OfType<BaseUser>().AsQueryable();
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return query.ToList();
        }

        public BaseUser GetById(int id)
        {
            return _applicationDbContext.Customers.FirstOrDefault(u => u.Id==id);
        }

        public void Update(BaseUser user)
        {
            _applicationDbContext.Customers.Update((Customer)user);
            _applicationDbContext.SaveChanges();
        }

        public BaseUser GetUser(string email, string password)
        {
            return _applicationDbContext.Customers.FirstOrDefault(u => u.Email == email && u.Password == password);
        }

        public BaseUser GetUserByEmail(string email)
        {
            return _applicationDbContext.Customers.FirstOrDefault(u => u.Email == email);
        }

        public async Task SaveChangesAsync()
        {
            await _applicationDbContext.SaveChangesAsync();
        }

        public IEnumerable<Customer> GetAllCustomers()
        {
            return _applicationDbContext.Customers.OfType<Customer>().ToList();
        }  
    }
}
