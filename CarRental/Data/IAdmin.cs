using CarRental.Models;
using System.Linq.Expressions;

namespace CarRental.Data
{
    public interface IAdmin
    {
            BaseUser GetById(int id);
            IEnumerable<BaseUser> GetAll();
            IEnumerable<BaseUser> GetAll(Expression<Func<BaseUser, bool>> filter = null);
            void Add(BaseUser user);
            void Update(BaseUser user);
            void Delete(BaseUser user);
            public BaseUser GetUser(string email, string password);
            public BaseUser GetUserByEmail(string email);
            Task SaveChangesAsync();
            public IEnumerable<Admin> GetAllAdmins();
    }
}
