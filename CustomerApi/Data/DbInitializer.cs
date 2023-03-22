using System.Collections.Generic;
using System.Linq;
using CustomerApi.Models;
using System;

namespace CustomerApi.Data
{
    public class DbInitializer : IDbInitializer
    {
        // This method will create and seed the database.
        public void Initialize(CustomerApiContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Look for any Customers
            if (context.Customers.Any())
            {
                return;   // DB has been seeded
            }

            List<Customer> customers = new List<Customer>
            {
                new Customer { Name = "Frank One", Email = "test@email.dk", Phone = "+4500000000", BillingAddress = "Fake Address Vej 01, 0000 Fake", ShippingAddress = "Fake Address Vej 01, 0000 Fake", CreditStanding = 200},
                new Customer { Name = "Frank Two", Email = "test@email.dk", Phone = "+4500000000", BillingAddress = "Fake Address Vej 01, 0000 Fake", ShippingAddress = "Fake Address Vej 01, 0000 Fake", CreditStanding = 200},
                new Customer { Name = "Frank Three", Email = "test@email.dk", Phone = "+4500000000", BillingAddress = "Fake Address Vej 01, 0000 Fake", ShippingAddress = "Fake Address Vej 01, 0000 Fake", CreditStanding = 200},
                new Customer { Name = "Frank Four", Email = "test@email.dk", Phone = "+4500000000", BillingAddress = "Fake Address Vej 01, 0000 Fake", ShippingAddress = "Fake Address Vej 01, 0000 Fake", CreditStanding = 200},
                new Customer { Name = "Frank Five", Email = "test@email.dk", Phone = "+4500000000", BillingAddress = "Fake Address Vej 01, 0000 Fake", ShippingAddress = "Fake Address Vej 01, 0000 Fake", CreditStanding = 200}
            };

            context.Customers.AddRange(customers);
            context.SaveChanges();
        }
    }
}