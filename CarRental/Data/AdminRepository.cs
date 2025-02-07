using CarRental.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CarRental.Data
{
    public class AdminRepository : IAdmin
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public AdminRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public void Add(BaseUser user)
        {
            _applicationDbContext.Admins.Add((Admin)user);
            _applicationDbContext.SaveChanges();
        }

        public void Delete(BaseUser user)
        {
            _applicationDbContext.Admins.Remove((Admin)user);
            _applicationDbContext.SaveChanges();
        }

        public IEnumerable<BaseUser> GetAll()
        {
            return _applicationDbContext.Admins.OrderBy(u => u.LastName);
        }

        public IEnumerable<BaseUser> GetAll(Expression<Func<BaseUser, bool>> filter)
        {
            var query = _applicationDbContext.Admins.OfType<BaseUser>().AsQueryable();
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return query.ToList();
        }

        public BaseUser GetById(int id)
        {
            return _applicationDbContext.Admins.FirstOrDefault(u => u.Id == id);
        }

        public void Update(BaseUser user)
        {
            _applicationDbContext.Admins.Update((Admin)user);
            _applicationDbContext.SaveChanges();
        }

        public BaseUser GetUser(string email, string password)
        {
            return _applicationDbContext.Admins.FirstOrDefault(u => u.Email == email && u.Password == password);
        }

        public BaseUser GetUserByEmail(string email)
        {
            return _applicationDbContext.Admins.FirstOrDefault(u => u.Email == email);
        }

        public async Task SaveChangesAsync()
        {
            await _applicationDbContext.SaveChangesAsync();
        }

        public IEnumerable<Admin> GetAllAdmins()
        {
            return _applicationDbContext.Admins.OfType<Admin>().ToList();
        }
    }
}