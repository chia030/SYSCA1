using CustomerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerApi.Data
{
    public class CustomerRepository : IRepository<Customer>
    {
        private readonly CustomerApiContext db;
        public CustomerRepository(CustomerApiContext context)
        {
            db = context;
        }

        IEnumerable<Customer> IRepository<Customer>.GetAll()
        {
            return db.Customers.ToList();
        }

        Customer IRepository<Customer>.Get(int id)
        {
            return db.Customers.FirstOrDefault(c => c.Id == id);
        }

        Customer IRepository<Customer>.Add(Customer entity)
        {
            var newCustomer = db.Customers.Add(entity).Entity;
            db.SaveChanges();
            return newCustomer;
        }

        void IRepository<Customer>.Update(Customer entity)
        {
            db.Entry(entity).State = EntityState.Modified;
            db.SaveChanges();
        }

        void IRepository<Customer>.Delete(int id)
        {
            var customer = db.Customers.FirstOrDefault(c => c.Id == id);
            db.Customers.Remove(customer);
            db.SaveChanges();
        }
    }
}
