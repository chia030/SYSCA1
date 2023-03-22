using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Data;
using OrderApi.Infrastructure;
using SharedModels;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        IOrderRepository repository;
        IServiceGateway<ProductDto> productServiceGateway;
        IServiceGateway<CustomerDto> customerServiceGateway;
        IMessagePublisher messagePublisher;

        public OrdersController(IRepository<Order> repos,
            IServiceGateway<ProductDto> gateway1, IServiceGateway<CustomerDto> gateway2,
            IMessagePublisher publisher)
        {
            repository = repos as IOrderRepository;
            productServiceGateway = gateway1;
            customerServiceGateway = gateway2;
            messagePublisher = publisher;
        }

        // GET orders
        [HttpGet]
        public IEnumerable<Order> Get()
        {
            return repository.GetAll();
        }

        // GET orders/5
        [HttpGet("{id}", Name = "GetOrder")]
        public IActionResult Get(int id)
        {
            var item = repository.Get(id);
            if (item == null)
            {
                return NotFound();
            }

            return new ObjectResult(item);
        }

        // POST orders
        [HttpPost]
        public IActionResult Post([FromBody] Order order)
        {
            if (order == null)
            {
                return BadRequest();
            }

            if (ProductItemsAvailable(order) & CustomerCreditPositive(order) & CustomerExists(order))
            {
                try
                {
                    // Publish OrderStatusChangedMessage. If this operation
                    // fails, the order will not be created
                    messagePublisher.PublishOrderStatusChangedMessage(
                        order.customerId, order.OrderLines, "completed");

                    // Create order.
                    order.Status = Order.OrderStatus.completed;
                    var newOrder = repository.Add(order);
                    return CreatedAtRoute("GetOrder", new { id = newOrder.Id }, newOrder);
                }
                catch
                {
                    return StatusCode(500, "An error happened. Try again.");
                }
            }
            else
            {
                // If there are not enough product items available.
                return StatusCode(500, "Not enough items in stock.");
            }
        }

        private bool ProductItemsAvailable(Order order)
        {
            foreach (var orderLine in order.OrderLines)
            {
                // Call product service to get the product ordered.
                var orderedProduct = productServiceGateway.Get(orderLine.ProductId);
                if (orderLine.Quantity > orderedProduct.ItemsInStock - orderedProduct.ItemsReserved)
                {
                    return false;
                }
            }

            return true;
        }

        private bool CustomerCreditPositive(Order order)
        {
            var customer = customerServiceGateway.Get(order.customerId); //either gateway or the fact customer id is nullable
            if (customer.CreditStanding < 0)
            {
                return false;
            }

            return true;
        }

        private bool CustomerExists(Order order)
        {
            var customer = customerServiceGateway.Get(order.customerId);
            if (customer == null)
            {
                return false;
            }

            return true;
        }

        private int TotalPaidPrice(Order order)
        {
            var totalPaid = 0;
            foreach (var orderLine in order.OrderLines)
            {
                // Call product service to get the product ordered.
                var orderedProduct = productServiceGateway.Get(orderLine.ProductId);
                totalPaid += (int) orderedProduct.Price;
            }

            return totalPaid;
        }

        // PUT orders/5/cancel
        // This action method cancels an order and publishes an OrderStatusChangedMessage
        // with topic set to "cancelled".
        [HttpPut("{id}/cancel")]
        public IActionResult Cancel(int id)
        {
            var order = repository.Get(id);
            if (order == null)
            {
                return NotFound();
            }

            try
            {
                messagePublisher.PublishOrderStatusChangedMessage(order.customerId, order.OrderLines,
                    topic: "cancelled");
                order.Status = Order.OrderStatus.cancelled;
                repository.Edit(order);
            }
            catch
            {
                return StatusCode(500, "An error happened. Try again.");
            }

            return StatusCode(200, "Order cancelled.");
        }

        // PUT orders/5/ship
        // This action method ships an order and publishes an OrderStatusChangedMessage.
        // with topic set to "shipped".
        [HttpPut("{id}/ship")]
        public IActionResult Ship(int id)
        {
            var order = repository.Get(id);
            if (order == null)
            {
                return NotFound();
            }

            try
            {
                order.Status = Order.OrderStatus.shipped;
                repository.Edit(order);
                messagePublisher.PublishOrderStatusChangedMessage(order.customerId, order.OrderLines, topic: "shipped");
            }
            catch
            {
                return StatusCode(500, "An error happened. Try again.");
            }

            return StatusCode(200, "Order shipped.");
        }

        // PUT orders/5/pay
        // This action method marks an order as paid and publishes a CreditStandingChangedMessage
        // (which have not yet been implemented), if the credit standing changes.
        [HttpPut("{id}/pay")]
        public IActionResult Pay(int id)
        {
            var order = repository.Get(id);
            if (order == null)
            {
                return NotFound();
            }

            try
            {
                order.Status = Order.OrderStatus.paid;
                repository.Edit(order);
                messagePublisher.PublishOrderStatusChangedMessage(order.customerId, order.OrderLines, topic: "paid");
                messagePublisher.PublishCreditStandingChangedMessage(order.customerId, TotalPaidPrice(order), topic: "paid");
            }
            catch
            {
                return StatusCode(500, "An error happened. Try again.");
            }

            return StatusCode(200, "Order shipped.");

        }
    }
}
