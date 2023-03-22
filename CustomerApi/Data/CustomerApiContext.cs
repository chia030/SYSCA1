using CustomerApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace CustomerApi.Data
{
    public class CustomerApiContext : DbContext
    {
        public CustomerApiContext(DbContextOptions<CustomerApiContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; } = null!;
    }
}