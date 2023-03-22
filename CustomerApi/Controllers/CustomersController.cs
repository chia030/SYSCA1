using CustomerApi.Data;
using CustomerApi.Models;
using Microsoft.AspNetCore.Mvc;
using SharedModels;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CustomerApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly IRepository<Customer> repository;
        private readonly IConverter<Customer, CustomerDTO> customerConverter;

        public CustomersController(IRepository<Customer> repos)
        {
            repository = repos;
        }
        // GET: customers
        [HttpGet]
        public IEnumerable<Customer> Get()
        {
            return repository.GetAll();
        }

        // GET customers/5
        [HttpGet("{id}", Name="GetCustomer")]
        public IActionResult Get(int id)
        {
            var item = repository.Get(id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        // POST customers
        [HttpPost]
        public IActionResult Post([FromBody] CustomerDTO customerDTO)
        {
            if (customerDTO == null)
            {
                return BadRequest();
            }

            var customer = customerConverter.Convert(customerDTO);
            var newCustomer = repository.Add(customer);

            return CreatedAtRoute("GetCustomer", new { id = newCustomer.Id }, customerConverter.Convert(newCustomer));

        }

        // PUT customers/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] CustomerDTO customerDto)
        {
            if (customerDto == null || customerDto.Id != id)
            {
                return BadRequest();
            }

            var modifiedCustomer = repository.Get(id);

            if (modifiedCustomer == null)
            {
                return NotFound();
            }

            modifiedCustomer.Name = customerDto.Name;
            modifiedCustomer.Email = customerDto.Email;
            modifiedCustomer.Phone = customerDto.Phone;
            modifiedCustomer.BillingAddress = customerDto.BillingAddress;
            modifiedCustomer.ShippingAddress = customerDto.ShippingAddress;
            modifiedCustomer.CreditStanding = customerDto.CreditStanding;

            repository.Update(modifiedCustomer);
            return new NoContentResult();
        }

        // DELETE customers/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (repository.Get(id) == null)
            {
                return NotFound();
            }

            repository.Delete(id);
            return new NoContentResult();
        }
    }
}
